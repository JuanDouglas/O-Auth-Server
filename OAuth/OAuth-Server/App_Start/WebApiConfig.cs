﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Web.Http;

namespace OAuth.Server
{
    public static class WebApiConfig
    {
        public static string[] KeyValueConnectionString { get => ConnectionString.Split(';'); }
        public static string EntityFreameworkConnectionString
        {
            get
            {
                try
                {
                    var connectionBuilder = new EntityConnectionStringBuilder
                    {
                        Provider = "System.Data.SqlClient",
                        ProviderConnectionString = $"{ConnectionString};MultipleActiveResultSets=True;App=EntityFramework;",
                        Metadata = @"res://*/Models.ShowProductsModel.csdl|res://*/Models.ShowProductsModel.ssdl|res://*/Models.ShowProductsModel.msl"
                    };
                    string connectionString = connectionBuilder.ToString();

                    return connectionString;
                }
                catch (Exception)
                {
                    return ConfigurationManager.ConnectionStrings["OAuthEntities"].ConnectionString;
                }
            }
        }
        public static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["OAuthDBServer"].ConnectionString; } }
        public static void Register(HttpConfiguration config)
        {
            // Serviços e configuração da API da Web

            // Rotas da API da Web
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
