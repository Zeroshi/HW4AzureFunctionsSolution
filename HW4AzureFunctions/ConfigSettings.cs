namespace HW4AzureFunctions
{
    public static class ConfigSettings
    {
        public const string GRAYSCALEIMAGES_CONTAINERNAME = "imagestoconverttograyscale";

        public const string CONVERTED_IMAGES_CONTAINERNAME = "convertedimages";

        public const string FAILED_IMAGES_CONTAINERNAME = "failedimages";

        public const string STORAGE_CONNECTIONSTRING_NAME = "AzureWebJobsStorage";

        public const string JOBS_TABLENAME = "imageconversionjobs";

        public const string IMAGEJOBS_PARTITIONKEY = "imageconversions";

        public const string JOBID_METADATA_NAME = "JobId";
        public const string SAS = "?sv=2020-02-10&ss=bfqt&srt=sco&sp=rwdlacupx&se=2021-04-30T04:00:00Z&st=2021-03-10T05:00:00Z&spr=https&sig=9PckxyGAsQ6SPwfataMspIqhyaH6UGZVI1EPO6OGuGo%3D";

    }
}
