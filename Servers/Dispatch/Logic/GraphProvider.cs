using GPAS.AccessControl;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.ServiceAccess.SearchService;
//using GPAS.Dispatch.ServiceAccess.RepositoryService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GPAS.Dispatch.Logic
{
    /// <summary>
    /// این کلاس با دسترسی به Repository Server جهت ذخیره و بازیابی چینش گراف را فراهم می کند.
    /// </summary>
    public class GraphProvider
    {
        private string CallerUserName = "";
        public GraphProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }

        /// <summary>
        /// این تابع چینش گراف ها را دریاقت کرده و در پایگاه داده ذخیره می کند.
        /// </summary>
        /// <param name="title">    .عنوان چینش گراف است    </param>
        /// <param name="description">   توضیح در  رابطه با چینش گراف است.   </param>
        /// <param name="graphImage">   تصویر چینش گراف است.    </param>
        /// <param name="graphArrangement">   این پارامتر فایل ایکس ام ال چینش گراف است.  </param>
        /// <param name="nodesCount">   این پارامتر تعداد نود ها ی موجود در گراف است.   </param>
        /// <param name="timeCreated">   این پارامتر زمان ایجاد گراف است   </param>
        /// <param name="dataSourceID">   این پارامتر شناسه منبع مربوط به گراف می‌باشد   </param
        /// <returns> یک شی از KGraphArrangement را بر می گرداند. </returns>
        public KGraphArrangement CreateGraphArrangement(long id, String title, string description, byte[] graphImage, byte[] graphArrangement, int nodesCount, string timeCreated, long dataSourceID)
        {
            //ServiceClient proxy = null;

            Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;

            try
            {
                //proxy = new ServiceClient();
                //EntityConvertor entityConvertor = new EntityConvertor();
                RepositoryEntityCreator repositoryEntityCreator = new RepositoryEntityCreator();
                //set TimeCreated For Graph


                serviceClient = new ServiceAccess.SearchService.ServiceClient();

                SearchGraphArrangement dbGraphArrangement = repositoryEntityCreator.CreateSearchGraphArrangement
                    (id, 
                    title, 
                    description,
                    graphImage,
                    graphArrangement, 
                    nodesCount, 
                    DateTime.Now.ToString(CultureInfo.InvariantCulture), 
                    dataSourceID
                    );

                //DBGraphArrangement dbGraphArrangementResult = proxy.CreateNewGraphArrangment(dbGraphArrangement);
                //KGraphArrangement kGraphArrangement = entityConvertor.ConvertDBGraphArrangementToKGraphArrangement(dbGraphArrangementResult);
                //return kGraphArrangement;


                var searchGraphArrangement = new Dispatch.ServiceAccess.SearchService.SearchGraphArrangement()
                {
                    Id = dbGraphArrangement.Id,
                    DataSourceID = dbGraphArrangement.DataSourceID,
                    Description = dbGraphArrangement.Description,
                    GraphArrangementXML = dbGraphArrangement.GraphArrangementXML,
                    GraphImage = dbGraphArrangement.GraphImage,
                    Title = dbGraphArrangement.Title,
                    NodesCount = dbGraphArrangement.NodesCount,
                    TimeCreated = dbGraphArrangement.TimeCreated
                };


                var result = serviceClient.SaveNew(searchGraphArrangement);

                return new KGraphArrangement()
                {
                    Id = result.Id,
                    DataSourceID = result.DataSourceID,
                    Title = result.Title,
                    TimeCreated = result.TimeCreated,
                    Description = result.Description,
                    GraphImage = result.GraphImage,
                    NodesCount = result.NodesCount,
                    GraphArrangement = result.GraphArrangementXML
                };

            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }

        }

        /// <summary>
        /// این تابع همه چینش گراف های موجود در پایگاه داده را بر می گرداند.    
        /// </summary>
        /// <returns>   لیستی از KGraphArrangement را بر می گرداند.    </returns>
        public List<KGraphArrangement> GetGraphArrangement()
        {
            //ServiceClient proxy = null;
            //try
            //{
            //    List<KGraphArrangement> resultKGraphArrangement = new List<KGraphArrangement>();
            //    List<DBGraphArrangement> resultDBGraphArrangement;
            //    EntityConvertor entityConvertor = new EntityConvertor();

            //    proxy = new ServiceClient();
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    resultDBGraphArrangement = proxy.GetGraphArrangements(authorizationParametter).ToList();
            //    foreach (var dbGraphArrangement in resultDBGraphArrangement)
            //    {
            //        KGraphArrangement kGraphArrangement = entityConvertor.ConvertDBGraphArrangementToKGraphArrangement(dbGraphArrangement);
            //        resultKGraphArrangement.Add(kGraphArrangement);
            //    }
            //    return resultKGraphArrangement;
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}

            Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                List<KGraphArrangement> resultKGraphArrangement = new List<KGraphArrangement>();
                List<Dispatch.ServiceAccess.SearchService.SearchGraphArrangement> resultDBGraphArrangement;
                EntityConvertor entityConvertor = new EntityConvertor();

                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                resultDBGraphArrangement = serviceClient.GetGraphArrangements(authorizationParametter).ToList();
                foreach (var dbGraphArrangement in resultDBGraphArrangement)
                {
                    KGraphArrangement kGraphArrangement = entityConvertor.ConvertDBGraphArrangementToKGraphArrangement(dbGraphArrangement);
                    resultKGraphArrangement.Add(kGraphArrangement);
                }
                return resultKGraphArrangement;
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }

        }

        /// <summary>
        /// این تابع تصویر متناسب با آیدی ورودی از کاربر را بر می گرداند.
        /// </summary>
        /// <param name="dbGraphArrangementID">  آیدی چینش گراف مورد نظر را از کاربر دریافت میکند.   </param>
        /// <returns>    تصویر را به صورت byte[] برمیگرداند.   </returns>
        public byte[] GetGraphImage(int kGraphArragementID)
        {
            //ServiceClient proxy = null;
            //try
            //{
            //    proxy = new ServiceClient();
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    byte[] graphImageResult = proxy.GetGraphImage(kGraphArragementID, authorizationParametter);
            //    return graphImageResult;
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}

            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                byte[] graphImageResult = serviceClient.GetGraphImage(kGraphArragementID, authorizationParametter);
                return graphImageResult;
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }

        }

        /// <summary>
        /// این تابع چینش XML متناسب با آیدی ورودی از کاربر را بر می گرداند.
        /// </summary>
        /// <param name="dbGraphArrangementID">  آیدی چینش گراف مورد نظر را از کاربر دریافت میکند.   </param>
        /// <returns>    چینش XML را به صورت byte[] برمیگرداند.   </returns>
        public byte[] GetGraphArrangementXML(int kGraphArragementID)
        {
            //ServiceClient proxy = null;
            //try
            //{
            //    proxy = new ServiceClient();
            //    AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            //    byte[] graphArrangementXMLResult = proxy.GetGraphArrangementXML(kGraphArragementID, authorizationParametter); ;
            //    return graphArrangementXMLResult;
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}

            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                AuthorizationParametters authorizationParametter = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

                byte[] graphArrangementXMLResult = serviceClient.GetGraphArrangementXML(kGraphArragementID, authorizationParametter); ;
                return graphArrangementXMLResult;
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }

        }

        public bool DeleteGraph(int graphId)
        {
            //ServiceClient proxy = null;
            //try
            //{
            //    proxy = new ServiceClient();
            //    bool check = proxy.DeleteGraph(graphId);
            //    return check;
            //}
            //finally
            //{
            //    if (proxy != null)
            //        proxy.Close();
            //}


            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient ServiceClient = null;
            try
            {
                ServiceClient = new ServiceAccess.SearchService.ServiceClient();
                bool check = ServiceClient.DeleteGraph(graphId);
                return check;
            }
            finally
            {
                if (ServiceClient != null)
                    ServiceClient.Close();
            }
        }
    }
}
