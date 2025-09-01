using Microsoft.AspNetCore.Mvc;

namespace ArshatidPublic.Classes
{
    public class ManualJwtSignInAttribute : TypeFilterAttribute
    {
        public ManualJwtSignInAttribute() : base(typeof(ManualJwtSignInFilter))
        {
        }
    }
}
