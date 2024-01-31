using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapTilesExporter.Logic
{
    public class ImageCacheManeger
    {
        string ConnectionString;
        string PathForSaveImages;
        const Int64 BlockSize = 1000;
        public ImageCacheManeger(string pathSqliteForExtract, string pathForSaveImages)
        {
            ConnectionString = "data source = " + @pathSqliteForExtract + "; Version=3;";
            PathForSaveImages = pathForSaveImages;
        }

        public bool GetImageFromCache()
        {
            try
            {
                using (SQLiteConnection cn = new SQLiteConnection())
                {
                    cn.ConnectionString = ConnectionString;
                    cn.Open();
                    Int64 count = 0;
                    using (DbCommand com = cn.CreateCommand())
                    {
                        string detachSqlQuery1 = "SELECT COUNT(*) FROM Tiles";
                        com.CommandText = detachSqlQuery1;
                        count = (Int64)com.ExecuteScalar();
                        Console.WriteLine("count is:  " + count);
                    }

                    Int64 page = 1;
                    DbDataReader reader;
                    
                    for (Int64 i = 1; i <= count; i++)
                    {
                        using (DbCommand com = cn.CreateCommand())
                        {
                            Int64 column = (page - 1) * BlockSize;
                            string detachSqlQuery = "select * from Tiles where id> " + column + " LIMIT "+ BlockSize.ToString();
                            com.CommandText = detachSqlQuery;
                            reader = com.ExecuteReader();
                            while (reader.Read())
                            {
                                string pathForType = PathForSaveImages + reader["Type"];
                                string pathForZoomLevel = pathForType + "\\Z" + reader["Zoom"];

                                if (!System.IO.Directory.Exists(pathForType))
                                    System.IO.Directory.CreateDirectory(pathForType);

                                if (!System.IO.Directory.Exists(pathForZoomLevel))
                                    System.IO.Directory.CreateDirectory(pathForZoomLevel);
                                using (DbCommand com1 = cn.CreateCommand())
                                {
                                    DbDataReader image;
                                    string tilesDataQuery = "select * from TilesData where id=" + reader["id"];
                                    com1.CommandText = tilesDataQuery;
                                    image = com1.ExecuteReader();
                                    image.Read();

                                    string imageName ="X"+ reader["X"].ToString() +"Y"+ reader["Y"].ToString() + ".png";
                                    if (!File.Exists((pathForZoomLevel + "\\" + imageName)))
                                        File.WriteAllBytes((pathForZoomLevel + "\\" + imageName), (byte[])image["Tile"]);
                                }
                            }
                        }
                        page++;
                        Console.WriteLine(page);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

    }
}
