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
using System.Xml.Serialization;

namespace eSpisLiteTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var profile = args.Length == 1 ? Profile.Load(args[0]) : Profile.Create(args);
            var client = new ObjectsManager
            {
                Url = profile.Url,
                Credentials = new System.Net.NetworkCredential(
                    userName: profile.Login,
                    password: profile.Password,
                    domain: profile.Domain
                )
            };

            Console.WriteLine("Hledat podle IČO/názvu [0-IČO] [1-Název] ?-");
            WsIdmOrgUnit ou = null;
            switch (Console.ReadLine())
            {
                case "0":
                    Console.WriteLine("Zadejte IČO ?-");
                    ou = FindOuByICO(client, Console.ReadLine(), profile.RootOU);
                    break;

                case "1":
                    Console.WriteLine("Zadejte název OU ?-");
                    ou = FindOuByName(client, Console.ReadLine(), profile.RootOU);
                    break;
            }

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

        static WsIdmOrgUnit FindOuByName(WsGenerated.ObjectsManager client, string ouName, int parentOU_ID)
        {
            var pagingInfo = new DbPagingInfo
            {
                Start = 0,
                Length = 10
            };

            var filters = new List<Filter>
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

            PrintXML(filters, "FindOuByName");

            var response = client.ReadOrgUnits(
                filters.ToArray(),
                ref pagingInfo,
                false
            );

            if (response.WsResults.Length == 1)
            {
                return response.WsResults[0];
            }

            return null;
        }

        static WsIdmOrgUnit FindOuByICO(WsGenerated.ObjectsManager client, string ico, int parentOU_ID)
        {
            var pagingInfo = new DbPagingInfo
            {
                Start = 0,
                Length = 10
            };

            var filters = new List<Filter>
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
                    Name = "ATTR_NAME=PO_ORGUNIT_ICO",
                    Operator = FilterOperator.In,
                    StringValues = new string[] { ico },
                    OperatorNext = FilterOperatorNext.And
                }
            };

            PrintXML(filters, "FindOuByICO");

            var response = client.ReadOrgUnits(
                filters.ToArray(),
                ref pagingInfo,
                false
            );

            if (response.WsResults.Length == 1)
            {
                return response.WsResults[0];
            }

            return null;
        }

        static void PrintXML(object req, string prefix)
        {
            Console.Out.WriteLine($"{prefix} -------------------------------");
            new XmlSerializer(req.GetType()).Serialize(Console.Out, req);
            Console.Out.WriteLine("-----------------------------------------------------");
            Console.Out.WriteLine();
        }

        static WsResponseOfWsIdmUser FindUsers(WsGenerated.ObjectsManager client, int ouID)
        {
            var pagingInfo = new DbPagingInfo
            {
                Start = 0,
                Length = 100
            };

            var filters = new List<Filter>
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
