using static HatchOS.HelperFunctions;

namespace HatchOS
{
    public static class BatchInterpreter
    {
        public static void InterpretBatchScript(string Script)
        {
            var MultilineScript = Script.Split('\n');

            foreach (var line in MultilineScript)
            {
                DisplayConsoleMsg(line);
            }
        } 
    }
}
