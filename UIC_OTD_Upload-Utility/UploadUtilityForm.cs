using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace UIC_OTD_Upload_Utility
{
    public partial class UploadUtlityForm : Form
    {
        UploadProgressForm progressForm;

        public UploadUtlityForm()
        {
            progressForm = new UploadProgressForm();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressForm.Show();

            string bucket = textBox1.Text;
            string file = textBox2.Text;

            Stopwatch watch = Stopwatch.StartNew();

            Upload(bucket, file);

            watch.Stop();
            TimeSpan ts = watch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            label5.Text = "Time Take to Upload = " + elapsedTime;
        }

        public void Upload(string bucket, string file)
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

                uploadRequest.UploadProgressEvent += new EventHandler<UploadProgressArgs>(uploadRequest_UploadPartProgressEvent);

                fileTransferUtility.Upload(uploadRequest);

                GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = "Tax presentation.pdf",
                    Expires = DateTime.Now.AddMinutes(200)
                };

                progressForm.label2.Text = "File Upload completed";

                string urlString = client.GetPreSignedURL(request1);

                textBox3.Text = urlString;

                Console.WriteLine("File Upload completed");

                progressForm.Hide();
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
        }

        void uploadRequest_UploadPartProgressEvent(object sender, UploadProgressArgs e)
        {
            if (progressBar1 == null)
            {
                Console.WriteLine("{0}/{1}", e.TransferredBytes, e.TotalBytes);
            }
            else
            {
                if (InvokeRequired)
                {
                    object[] args = new object[] { sender, e };
                    Invoke((MethodInvoker)delegate { uploadRequest_UploadPartProgressEvent(sender, e); } );
                }
                else
                {
                    int val = (int)((e.TransferredBytes / e.TotalBytes) * 100);
                    progressForm.progressBar1.Value = val;
                    progressForm.label1.Text = String.Format("{0}% Upload Done", val);
                }
            }
        }
    }
}
