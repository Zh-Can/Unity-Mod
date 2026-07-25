namespace ZaoHuaBMod.GuiFramework.Logger
{
    public interface ILogger
    {
        void Info(object msg);
        void Warning(object msg);
        void Error(object msg);
    }
}
