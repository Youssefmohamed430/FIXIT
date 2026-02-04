namespace FIXIT.Presentation.Attributes 
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheableAttribute : Attribute
    {
        public string Key { get; }

        public CacheableAttribute(string key)
        {
            Key = key;
        }
    }
}
