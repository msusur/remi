using ReMi.BusinessEntities.Auth;

namespace ReMi.Queries.Auth
{
    public class GetActiveSessionResponse
    {
        public Account Account { get; set; }

        public Session Session { get; set; }

        public string Token { get; set; }

        public override string ToString()
        {
            return string.Format("[Account = {0}, Session = {1}, Token = {2}]", Account,
                Session, Token);
        }
    }
}
