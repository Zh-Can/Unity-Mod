namespace ZaoHuaBMod.UI.Logger
{
    public interface ILogger
    {
        void Info(object msg);
        void Warning(object msg);
        void Error(object msg);
    }
}
