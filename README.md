# InlineSQL-Solution
 Support library to using XML to manage your inline sql's targeting multiple database vendors

# Binding SQL XML using attributes to the respective data access classes
[InLineSQL.SqlScriptAttribute("InlineSQL.SampleDataAccessLayer.Sql.UserData.xml", typeof(UserConnect))]
public class UserConnect

# Accessing SQL script within the data access class
string sql = this.MySqlScriptor.GetSql("<Pass in the script>");
