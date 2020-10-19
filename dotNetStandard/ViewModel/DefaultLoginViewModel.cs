using Atomus.Page.Login.Controllers;
using Atomus.Security;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Atomus.Page.Login.ViewModel
{
    public class DefaultLoginViewModel : MVVM.ViewModel
    {
        #region Declare
        private ICore core;
        private readonly string Configfilename;

        private string email;
        private string accessNumber;
        private bool rememberEmailIsToggled;
        private bool autoLoginIsToggled;
        private bool activityIndicator;
        private bool isEnabledControl;

        private ImageSource backgroundImageHorizontal;
        private ImageSource backgroundImageVertical;
        private Aspect backgroundImageAspect;

        private DisplayOrientation deviceDirection;
        #endregion

        #region Property
        public Application Application { get; set; }
        public ICore Core
        {
            get
            {
                return this.core;
            }
            set
            {
                this.core = value;
                this.ConfigLoad();
            }
        }

        private Xamarin.Forms.Page JoinPage { get; set; }

        public string Eemail
        {
            get
            {
                return this.email;
            }
            set
            {
                if (this.email != value)
                {
                    this.email = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string AccessNumber
        {
            get
            {
                return this.accessNumber;
            }
            set
            {
                if (this.accessNumber != value)
                {
                    this.accessNumber = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool RememberEmailIsToggled
        {
            get
            {
                return this.rememberEmailIsToggled;
            }
            set
            {
                if (this.rememberEmailIsToggled != value)
                {
                    if (!value)
                        this.AutoLoginIsToggled = value;

                    this.rememberEmailIsToggled = value;

                    NotifyPropertyChanged();

                    this.ConfigSave();
                }
            }
        }
        public bool AutoLoginIsToggled
        {
            get
            {
                return this.autoLoginIsToggled;
            }
            set
            {
                if (this.autoLoginIsToggled != value)
                {
                    if (value)
                        this.RememberEmailIsToggled = value;

                    this.autoLoginIsToggled = value;

                    NotifyPropertyChanged();

                    this.ConfigSave();
                }
            }
        }
        public bool ActivityIndicator
        {
            get
            {
                return this.activityIndicator;
            }
            set
            {
                if (this.activityIndicator != value)
                {
                    this.activityIndicator = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsEnabledControl
        {
            get
            {
                return this.isEnabledControl;
            }
            set
            {
                if (this.isEnabledControl != value)
                {
                    this.isEnabledControl = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ImageSource BackgroundImage
        {
            get
            {
                if (this.deviceDirection == DisplayOrientation.Landscape)
                    return this.backgroundImageHorizontal;
                else
                    return this.backgroundImageVertical;
            }
            set
            {
                if (this.deviceDirection == DisplayOrientation.Landscape)
                {
                    this.backgroundImageHorizontal = value;
                    NotifyPropertyChanged();
                }
                else
                {
                    this.backgroundImageVertical = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DisplayOrientation DeviceDirection
        {
            get
            {
                return this.deviceDirection;
            }
            set
            {
                if (this.deviceDirection != value)
                {
                    this.deviceDirection = value;

                    if (this.backgroundImageHorizontal == null || this.backgroundImageVertical == null)
                        this.GetBackgroundImage();

                    if (this.deviceDirection == DisplayOrientation.Landscape)
                        this.BackgroundImage = this.backgroundImageHorizontal;
                    else
                        this.BackgroundImage = this.backgroundImageVertical;

                    NotifyPropertyChanged();
                }
            }
        }

        private async void GetBackgroundImage()
        {
            this.backgroundImageHorizontal = await this.core.GetAttributeWebImage("BackgroundImage.Horizontal");
            this.backgroundImageVertical = await this.core.GetAttributeWebImage("BackgroundImage.Vertical");

            if (this.deviceDirection == DisplayOrientation.Landscape)
                this.BackgroundImage = this.backgroundImageHorizontal;
            else
                this.BackgroundImage = this.backgroundImageVertical;
        }

        public Aspect BackgroundImageAspect
        {
            get
            {
                return this.backgroundImageAspect;
            }
            set
            {
                if (this.backgroundImageAspect != value)
                {
                    this.backgroundImageAspect = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand LoginCommand { get; set; }
        public ICommand JoinCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        #endregion

        #region INIT
        public DefaultLoginViewModel()
        {
            this.email = "";
            this.accessNumber = "";
            this.autoLoginIsToggled = false;
            this.autoLoginIsToggled = false;
            this.activityIndicator = false;
            this.isEnabledControl = true;
            this.backgroundImageHorizontal = null;
            this.backgroundImageVertical = null;
            this.backgroundImageAspect = Aspect.AspectFit;

            this.Configfilename = Path.Combine(Factory.FolderPath, $"DefaultLoginStandard.config");

            this.LoginCommand = new Command(async () => await this.LoginProcess()
                                            , () => { return !this.ActivityIndicator; });

            this.JoinCommand = new Command(async () => await this.JoinProcess()
                                            , () => { return !this.ActivityIndicator; });

            this.ExitCommand = new Command(async () => await this.Exit()
                                            , () => { return !this.ActivityIndicator; });


            this.deviceDirection = DisplayOrientation.Unknown;
        }
        public DefaultLoginViewModel(ICore core) : this()
        {
            this.Core = core;
        }
        #endregion

        #region IO
        private void ConfigLoad()
        {
            IDecryptor decryptor;

            try
            {
                if (File.Exists(this.Configfilename))
                {
                    string tmp = File.ReadAllText(Configfilename);
                    string[] tmps = tmp.Split(',');

                    this.rememberEmailIsToggled = tmps[0].Split(':')[1].Length > 0;
                    this.autoLoginIsToggled = tmps[1].Split(':')[1].Length > 0;

                    if (this.rememberEmailIsToggled)
                        this.email = tmps[0].Split(':')[1];

                    if (this.autoLoginIsToggled && this.email.Length > 0 && tmps[0].Split(':')[1].Length > 0)
                    {
                        decryptor = (IDecryptor)this.Core.CreateInstance("Decryptor");

                        this.accessNumber = decryptor.DecryptFromBase64String(tmps[1].Split(':')[1], this.Eemail, this.Eemail);
                    }
                }
                else
                {
                    this.ConfigSave();
                }
            }
            catch (Exception ex)
            {
                Diagnostics.DiagnosticsTool.MyTrace(ex);
            }
        }
        private void ConfigSave()
        {
            IEncryptor encryptor;

            try
            {
                if (this.Eemail.Length > 0 && this.AccessNumber.Length > 0)
                {
                    if (this.RememberEmailIsToggled && this.AutoLoginIsToggled)
                    {
                        encryptor = (IEncryptor)this.Core.CreateInstance("Encryptor");

                        File.WriteAllText(this.Configfilename, $"EMAIL:{(this.RememberEmailIsToggled ? this.Eemail : "")},ACCESS_NUMBER:{encryptor.EncryptToBase64String(this.AccessNumber, this.Eemail, this.Eemail)}");
                    }
                    else if (this.RememberEmailIsToggled)
                    {
                        File.WriteAllText(this.Configfilename, $"EMAIL:{(this.RememberEmailIsToggled ? this.Eemail : "")},ACCESS_NUMBER:");
                    }
                    else
                        File.WriteAllText(this.Configfilename, $"EMAIL:,ACCESS_NUMBER:");
                }
                else
                    File.WriteAllText(this.Configfilename, $"EMAIL:,ACCESS_NUMBER:");
            }
            catch (Exception ex)
            {
                Diagnostics.DiagnosticsTool.MyTrace(ex);
                File.WriteAllText(this.Configfilename, $"EMAIL:,ACCESS_NUMBER:");
            }
        }

        NavigationPage navigationPage;
        private async Task LoginProcess()
        {
            Service.IResponse result;
            ISecureHashAlgorithm secureHashAlgorithm;
            string tmp;

            try
            {
                this.IsEnabledControl = false;
                this.ActivityIndicator = true;
                (this.LoginCommand as Command).ChangeCanExecute();

                secureHashAlgorithm = (ISecureHashAlgorithm)this.core.CreateInstance("SecureHashAlgorithm");

                result = await this.core.SearchAsync(this.email, secureHashAlgorithm.ComputeHashToBase64String(this.accessNumber));

                if (result.Status == Service.Status.OK)
                {
                    this.ConfigSave();

                    if (result.DataSet != null && result.DataSet.Tables.Count >= 1)
                        foreach (DataTable _DataTable in result.DataSet.Tables)
                            for (int i = 1; i < _DataTable.Columns.Count; i++)
                                foreach (DataRow _DataRow in _DataTable.Rows)
                                {
                                    tmp = string.Format("{0}.{1}", _DataRow[0].ToString(), _DataTable.Columns[i].ColumnName);

                                    if (Config.Client.GetAttribute(tmp) == null)
                                        Config.Client.SetAttribute(tmp, _DataRow[i]);
                                }


                    if (this.Application.MainPage is NavigationPage)
                    {
                        this.Application.MainPage = (Xamarin.Forms.Page)this.core.CreateInstance("DefaultBrowser");
                        //this.Application.MainPage = new Browser.DefaultBrowser();
                    }
                    else
                        await this.Exit(false);


                    //await Application.Current.MainPage.DisplayAlert("Warning", this.Application.MainPage.ToString(), "OK");

                    //Factory.Dispose(this);

                    //if (this.Application.MainPage is NavigationPage)
                    //{
                    //    this.navigationPage = (this.Application.MainPage as NavigationPage);

                    //    Xamarin.Forms.NavigationPage.SetHasNavigationBar((this.Core as Xamarin.Forms.Page), false);

                    //    await Application.Current.MainPage.DisplayAlert("Warning1", this.navigationPage.CurrentPage.ToString(), "OK");

                    //    Factory.Dispose((ICore)this.navigationPage.CurrentPage);

                    //    await this.navigationPage.PopAsync();
                    //    await Application.Current.MainPage.DisplayAlert("Warning2", this.navigationPage.CurrentPage.ToString(), "OK");

                    //    await this.navigationPage.PushAsync((Xamarin.Forms.Page)this.core.CreateInstance("DefaultBrowser"));

                    //    //this.Application.MainPage = null;
                    //    //this.Application.MainPage = (Xamarin.Forms.Page)this.core.CreateInstance("DefaultBrowser");
                    //    //this.Application.MainPage = new Browser.DefaultBrowser();
                    //}
                    //else
                    //{



                    //    await Application.Current.MainPage.DisplayAlert("Warning", "2", "OK");

                    //    await this.navigationPage.PopAsync();

                    //    await this.navigationPage.PushAsync((Xamarin.Forms.Page)this.core.CreateInstance("DefaultBrowser"));

                    //    //this.Application.MainPage = this.navigationPage;
                    //    //this.Application.MainPage = (Xamarin.Forms.Page)this.core.CreateInstance("DefaultBrowser");
                    //}
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Warning", result.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Warning", ex.Message, "OK");
            }
            finally
            {
                this.ActivityIndicator = false;
                this.IsEnabledControl = true;
                (this.LoginCommand as Command).ChangeCanExecute();
            }
        }

        private async Task JoinProcess()
        {
            try
            {
                this.IsEnabledControl = false;
                this.ActivityIndicator = true;
                (this.JoinCommand as Command).ChangeCanExecute();

                if (this.JoinPage == null)
                {
                    this.JoinPage = (Xamarin.Forms.Page)this.core.CreateInstance("DefaultJoin");
                    //this.JoinPage = new Join.DefaultJoinStandard();
                }

                await ((NavigationPage)this.Application.MainPage).PushAsync(this.JoinPage);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Warning", ex.Message, "OK");
            }
            finally
            {
                this.ActivityIndicator = false;
                this.IsEnabledControl = true;
                (this.JoinCommand as Command).ChangeCanExecute();
            }
        }

        internal async Task Exit(bool isQuestions = true)
        {
            bool result;

            this.IsEnabledControl = false;
            this.ActivityIndicator = true;
            (this.ExitCommand as Command).ChangeCanExecute();

            if (isQuestions)
                result = await Application.Current.MainPage.DisplayAlert("Exit", "Do you wan't to exit the App?", "Yes", "No");
            else
                result = true;

            if (result)
            {
                DependencyService.Get<INativeHelper>().CloseApp();
            }

            this.ActivityIndicator = false;
            this.IsEnabledControl = true;
            (this.ExitCommand as Command).ChangeCanExecute();
        }


        private async Task<ImageSource> ImageSourceDownloadAsyn(string url)
        {
            byte[] vs;
            ImageSource imageSource;
            var client = new System.Net.WebClient();

            vs = await client.DownloadDataTaskAsync(new Uri(url));

            imageSource = ImageSource.FromStream(() => new MemoryStream(vs));

            return imageSource;
        }


        private void ImageDownload(string url, string filename)
        {
            string path;

            var client = new System.Net.WebClient();
            var data = client.DownloadData(url);

            path = Path.Combine(Factory.FolderPath, filename);

            System.IO.File.WriteAllBytes(path, data);
        }
        #endregion


        #region ETC
        #endregion
    }
}