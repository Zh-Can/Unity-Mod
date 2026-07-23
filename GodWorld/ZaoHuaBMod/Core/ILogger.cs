namespace ZaoHuaBMod.Core
{
    public interface ILogger
    {
        void Info(object msg);
        void Warning(object msg);
        void Error(object msg);
    }
}
