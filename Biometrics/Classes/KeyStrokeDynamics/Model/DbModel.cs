using System.Collections.Generic;

namespace Biometrics.Classes.KeyStrokeDynamics.DBConnection
{
    public class DbModel
    {
        public int Id { get; set; }
        public List<string> ConstTextList { get; set; }
        public List<string> AnyText { get; set; }
    }
}