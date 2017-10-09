namespace Pelco.Media.Pipeline
{
    public interface IObjectTypeSink<T>: ISink
    {
        bool HandleObject(T obj);
    }
}
