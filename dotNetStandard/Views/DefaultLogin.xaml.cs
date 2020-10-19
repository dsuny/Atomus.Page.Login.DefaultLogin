using Atomus.Page.Login.ViewModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Atomus.Page.Login
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DefaultLogin : ContentPage, ICore
    {
        #region INIT
        public DefaultLogin()
        {
            this.BindingContext = new DefaultLoginViewModel(this);

            InitializeComponent();

            this.BackgroundColor = ((string)Config.Client.GetAttribute("BackgroundColor")).ToColor();
        }
        public DefaultLogin(Application application) : this()
        {
            if (this.BindingContext != null)
                (this.BindingContext as DefaultLoginViewModel).Application = application;
        }
        #endregion

        #region EVENT
        private bool IsFirst = false;
        protected override void OnAppearing()
        {
            //if (!this.IsFirst)
                this.AutoLoaginAsync();

            this.IsFirst = true;
        }
        private async void AutoLoaginAsync()
        {
            bool result;

            if ((this.BindingContext as DefaultLoginViewModel).AutoLoginIsToggled)
            {
                //result = await Application.Current.MainPage.DisplayAlert("Login", "자동 로그인 하시겠습니까??", "Yes", "No");

                //if (result)
                (this.BindingContext as DefaultLoginViewModel).LoginCommand.Execute(null);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            this.Exit();
            return true;
        }

        private DisplayOrientation DisplayOrientation = DisplayOrientation.Unknown;
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height); //must be called

            if (this.DisplayOrientation != DeviceDisplay.MainDisplayInfo.Orientation)
            {
                if (this.BindingContext != null)
                    (this.BindingContext as DefaultLoginViewModel).DeviceDirection = DeviceDisplay.MainDisplayInfo.Orientation;

                this.DisplayOrientation = DeviceDisplay.MainDisplayInfo.Orientation;
            }
        }
        #endregion

        #region ETC
        private async void Exit()
        {
            await (this.BindingContext as DefaultLoginViewModel).Exit();
        }
        #endregion
    }
}