namespace Epson.Model.Common
{
    public class BaseQueryModel<TModel>
    {
        public BaseQueryModel()
        {
            Type t = typeof(TModel);
            if (t.GetConstructor(Type.EmptyTypes) != null)
                Data = Activator.CreateInstance<TModel>();
        }

        public TModel Data { get; set; }
    }

}
