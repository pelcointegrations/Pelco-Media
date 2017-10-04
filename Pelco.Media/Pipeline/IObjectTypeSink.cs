namespace Pelco.PDK.Media.Pipeline
{
    public interface IObjectTypeSink<T>: ISink
    {
        bool HandleObject(T obj);
    }
}
