namespace InlineSQL.SampleDataAccessLayer
{
    [InLineSQL.SqlScriptAttribute("InlineSQL.SampleDataAccessLayer.Sql.UserData.xml", typeof(UserConnect))]
    public class UserConnect
    {
        public InLineSQL.SqlScript MySqlScriptor { get; private set; }

        public UserConnect() {
            //Look for the script attribute and intialize the script access class
            //Ideally this code should be positioned within a based class to which this class should inherite.
            object[] attributes = { this.GetType().GetCustomAttributes(typeof(InLineSQL.SqlScriptAttribute), true) };

            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    InLineSQL.SqlScriptAttribute[] sqlAttributes = (InLineSQL.SqlScriptAttribute[])attributes[0];
                    if (sqlAttributes.Length > 0)
                    {
                        this.MySqlScriptor = new InLineSQL.SqlScript(sqlAttributes[0].ScriptNamespace,
                            sqlAttributes[0].ContainingAssembly);

                        break;
                    }
                }
            }
        }

        public void QueryUserInfor(string userid)
        {
            //Retrieve the sql by passing the sql key name. This woould retrieve the appropriate sql script
            //based on the configured database.
            string sql = this.MySqlScriptor.GetSql("QueryUserInformation");
        }
    }
}
