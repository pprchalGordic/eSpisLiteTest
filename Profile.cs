//  <FileHeader xmlns="http://www.gordic.cz/shared/file-header/v_1.0.0.0">
//    <Name>        eSpisLiteTest.Class1.cs                                     </Name>
//    <Description> Login profile for eSpisLite                                 </Description>
//    <Author>      Pavel Prchal                                                </Author>
//    <Copyright>   © GORDIC spol. s r. o. 1993-2025                            </Copyright>
//    <Created>     2025-05-14                                                  </Created>
//  </FileHeader>

using System;
using System.IO;

namespace eSpisLiteTest
{
    /// <summary>
    /// Login profile for eSpisLite
    /// </summary>
    public sealed class Profile
    {
        public string Login { get; private set; }
        public string Domain { get; private set; }
        public string Password { get; private set; }
        public string Url { get; private set; }
        public int RootOU { get; private set; }
        public int RoleID { get; private set; }

        public static Profile Load(string fileName)
        {
            var lines = File.ReadAllLines(fileName);
            if (lines.Length != 6)
            {
                throw new ArgumentException($"File {fileName} must contain 6 lines");
            }

            return Create(lines);
        }

        public static Profile Create(string[] args)
        {
            return new Profile
            {
                Url = args[3],
                Login = args[1],
                Password = args[2],
                Domain = args[0],
                RootOU = int.Parse(args[4]),
                RoleID = int.Parse(args[5])
            };
        }
    }
}
