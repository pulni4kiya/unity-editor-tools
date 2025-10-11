using UnityEngine;

namespace Pulni.EditorTools {

    [TypePickerInfo("Frequency/Once", 1000)]
    public class PassOnceCondition : IAdvancedCondition {
        private bool passed = false;
        public bool CheckCondition(ExecutionContext context) {
            if (this.passed) {
                return false;
            }
            this.passed = true;
            return true;
        }

        public string GetDescription() {
            return "pass once";
        }
    }

    [TypePickerInfo("Frequency/Every X Checks", 1000)]
    public class PassEveryChecksCondition : IAdvancedCondition {
        [SerializeField] private int every;

        private int count = 0;
        public bool CheckCondition(ExecutionContext context) {
            var result = count % every == 0;
            this.count++;
            return result;
        }

        public string GetDescription() {
            return $"pass once in {every} checks";
        }
    }

    [TypePickerInfo("Frequency/Every X Seconds", 1000)]
    public class PassEverySecondsCondition : IAdvancedCondition {
        [SerializeField] private float seconds;
        private double time = double.MinValue;
        public bool CheckCondition(ExecutionContext context) {
            if (Time.realtimeSinceStartupAsDouble > time) {
                time = Time.realtimeSinceStartupAsDouble + seconds;
                return true;
            }
            return false;
        }

        public string GetDescription() {
            return $"pass once in {seconds} seconds";
        }
    }

    [TypePickerInfo("Frequency/By Chance", 1000)]
    public class PassByChanceCondition : IAdvancedCondition {
        [SerializeField] private float chance;
        public bool CheckCondition(ExecutionContext context) {
            return UnityEngine.Random.value < chance;
        }

        public string GetDescription() {
            return $"pass by chance {chance:P2}";
        }
    }
}
