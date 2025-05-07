using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Actuator.Flume.Editor
{
    public class TestListerner
    {
        [RuntimeInitializeOnLoadMethod]
        public static void SetupTestListeners()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new CleanStaticsOnTestFinished());
        }

        private class CleanStaticsOnTestFinished : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                // Method intentionally left empty.
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                // Method intentionally left empty.
            }

            public void TestStarted(ITestAdaptor test)
            {
                // Method intentionally left empty.
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                FlumeServiceContainer.EditorCleanStatics();
            }
        }
    }
}