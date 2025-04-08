//  <FileHeader xmlns="http://www.gordic.cz/shared/file-header/v_1.0.0.0">
//    <Name>        eSpisLiteTest.Program.cs                                    </Name>
//    <Description> FindOU                                                      </Description>
//    <Author>      Pavel Prchal                                                </Author>
//    <Copyright>   © GORDIC spol. s r. o. 1993-2025                            </Copyright>
//    <Created>     2025-04-08                                                  </Created>
//  </FileHeader>

using eSpisLiteTest.WsGenerated;
using System;
using System.Collections.Generic;

namespace eSpisLiteTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var client = new ObjectsManager
            {
                Url = args[3],
                Credentials = new System.Net.NetworkCredential(
                    userName: args[1],
                    password: args[2],
                    domain: args[0]
                )
            };
            var rootOU = int.Parse(args[4]);

            // Bellevue, poskytovatel sociálních služeb
            Console.WriteLine("Zadejte název OU");
            var ouName = Console.ReadLine();
            var ou = FindOU(client, ouName, rootOU);
            if (ou != null)
            {
                Console.Out.WriteLine("OU: " + ou.Id + ", " + ou.Name);
                foreach (var user in FindUsers(client, ou.Id).WsResults)
                {
                    Console.WriteLine($"User[{user.Id}-{user.IdmLogin}] {user.Jmeno} {user.Prijmeni}");
                    foreach (var role in user.Roles)
                    {
                        Console.WriteLine($"\t Role[{role.Id}-{role.Name}] {role.RoleType}");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.Out.WriteLine("OU not found");
            }
        }

        static WsIdmOrgUnit FindOU(WsGenerated.ObjectsManager client, string ouName, int parentOU_ID)
        {
            var pagingInfo = new DbPagingInfo
            {
                Start = 0,
                Length = 10
            };

            var filters = new List<Filter>()
            {
                new Filter
                {
                    Name = "IDM_ORGUNIT_PARENT_ID",
                    Operator = FilterOperator.Equals,
                    IntegerValues = new long[] { parentOU_ID },
                    OperatorNext = FilterOperatorNext.And
                },

                new Filter
                {
                    Name = "ATTR_NAME=IDM_ORGUNIT_NAME",
                    Operator = FilterOperator.In,
                    StringValues = new string[] { ouName },
                    OperatorNext = FilterOperatorNext.And
                }
            };

            var response = client.ReadOrgUnits(
                filters.ToArray(),
                ref pagingInfo,
                false
            );

            return response.WsResults[0];
        }

        static WsResponseOfWsIdmUser FindUsers(WsGenerated.ObjectsManager client, int ouID)
        {
            var pagingInfo = new DbPagingInfo
            {
                Start = 0,
                Length = 100
            };

            var filters = new List<Filter>()
            {
                new Filter
                {
                    Name = "ORGUNIT",
                    Operator = FilterOperator.In,
                    IntegerValues = new long[] { ouID },
                    OperatorNext = FilterOperatorNext.And
                }
            };

            return client.ReadUsers(
                filters: filters.ToArray(),
                pagingInfo: ref pagingInfo,
                async: false
            );
        }
    }
}
