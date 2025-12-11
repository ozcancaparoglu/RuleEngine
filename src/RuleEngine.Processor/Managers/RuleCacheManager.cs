using RuleEngine.Compiler;
using RuleEngine.Core;

namespace RuleEngine.Processor.Managers;

public class RuleCacheManager
    {
        private readonly RuleCompiler _compiler;
        private readonly ILogger<RuleCacheManager> _logger;

        // VOLATILE: Ensures that when this reference is updated by the Writer thread (SyncWorker),
        // all Reader threads (API requests) immediately see the new list.
        private volatile List<ExecutableRule> _activeRules = new();

        public RuleCacheManager(RuleCompiler compiler, ILogger<RuleCacheManager> logger)
        {
            _compiler = compiler;
            _logger = logger;
        }

        /// <summary>
        /// Fast access to the current rules. 
        /// Returns the reference to the list. 
        /// Reading is O(1) and Lock-Free.
        /// </summary>
        public List<ExecutableRule> GetActiveRules()
        {
            return _activeRules;
        }

        /// <summary>
        /// Compiles and replaces the rule set.
        /// This is the "Slow" path run by the Background Worker.
        /// </summary>
        public void ReloadRules(IEnumerable<RuleDefinition> ruleDefinitions)
        {
            _logger.LogInformation("Starting rule compilation and reload...");

            var newRules = new List<ExecutableRule>();
            int failureCount = 0;

            foreach (var def in ruleDefinitions)
            {
                // Skip disabled rules immediately
                if (!def.IsEnabled) continue;

                try
                {
                    // 1. Compile the string expression into a Delegate
                    var compiledFunc = _compiler.CompileRule(def.Expression);

                    // 2. Create the executable object
                    var executableRule = new ExecutableRule
                    {
                        RuleId = def.Id,
                        RuleName = def.RuleName,
                        Priority = def.Priority,
                        CompiledCondition = compiledFunc,
                        SuccessActions = def.SuccessActions ?? new List<RuleAction>()
                    };

                    newRules.Add(executableRule);
                }
                catch (Exception ex)
                {
                    // CRITICAL: Catch compilation errors per rule. 
                    // Do not let one bad rule prevent the others from loading.
                    _logger.LogError(ex, "Failed to compile rule '{RuleId}' ({RuleName}). Skipping.", def.Id, def.RuleName);
                    failureCount++;
                }
            }

            // 3. Optimization: Sort by Priority once.
            // Higher priority (e.g. 100) comes before Lower priority (e.g. 1).
            var sortedRules = newRules.OrderByDescending(r => r.Priority).ToList();

            // 4. ATOMIC SWAP
            // This assignment is atomic in .NET. Readers will either see the OLD list or the NEW list.
            // They will never see a partial list or null.
            _activeRules = sortedRules;

            _logger.LogInformation("Rules Reloaded. Active: {Count}. Failed: {Failed}.", _activeRules.Count, failureCount);
        }
    }