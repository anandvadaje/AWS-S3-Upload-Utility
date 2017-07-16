using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace AWS_S3_Upload_Utility
{
    public partial class UploadUtlityForm : Form
    {
        string bucket = "";
        string file = "";
        string url = "";
        string elapsedTime = "";
        
        public UploadUtlityForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bucket = textBox1.Text;
            file = textBox2.Text;
            
            pictureBox1.Visible = true;
            backgroundWorker.RunWorkerAsync();
            
        }

        public string Upload(string bucket, string file)
        {
            string bucketName = bucket;
            string filePath = file;
            var client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);
            
            try
            {
                TransferUtility fileTransferUtility = new TransferUtility(client);
                fileTransferUtility.Upload(filePath, bucketName);

                // Use TransferUtilityUploadRequest to configure options.
                // In this example we subscribe to an event.
                TransferUtilityUploadRequest uploadRequest =
                    new TransferUtilityUploadRequest
                    {
                        BucketName = bucketName,
                        FilePath = filePath,
                    };

                uploadRequest.UploadProgressEvent += new EventHandler<UploadProgressArgs>(uploadProgress);

                fileTransferUtility.Upload(uploadRequest);

                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = Path.GetFileName(filePath),
                    Expires = DateTime.Now.AddMinutes(200)
                };
                
                string urlString = client.GetPreSignedURL(request1);
                Console.WriteLine("File Upload completed");

                return urlString;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }

            void uploadProgress(object sender, UploadProgressArgs e)
            {
                int pctProgress = (int)(e.TransferredBytes * 100 / e.TotalBytes);
                backgroundWorker.ReportProgress(pctProgress);
                
                Console.WriteLine(e.TransferredBytes + " / " + e.TotalBytes + Environment.NewLine);
            }
        }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Stopwatch watch = Stopwatch.StartNew();
            url = Upload(bucket, file);
            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
        }

        private void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (pictureBox1.Visible)
            {
                pictureBox1.Visible = false;    
            }
                
            progressBar1.Value = e.ProgressPercentage;
            label4.Text = e.ProgressPercentage + "% Upload Completed .....";
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            label4.Text = "File Upload Completed !!";
            textBox3.Text = url;
            label5.Text = "Upload Time: " + elapsedTime;
            label5.Visible = true;
        }
    }
}
