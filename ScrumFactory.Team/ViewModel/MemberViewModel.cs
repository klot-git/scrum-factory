using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ScrumFactory.Composition.ViewModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using ScrumFactory.Composition;
using ScrumFactory.Services;
using System.Collections.ObjectModel;
using ScrumFactory.Composition.View;

namespace ScrumFactory.Team.ViewModel {

    [Export(typeof(MemberViewModel))]
    [Export(typeof(ITopMenuViewModel))]
    public class MemberViewModel : BaseEditableObjectViewModel, ITopMenuViewModel {

        private bool isMemberSelected = false;

        private IBackgroundExecutor executor;
        private IAuthorizationService authorizator;
        private IEventAggregator aggregator;
        private ITeamService teamService;
        private ITasksService taskService;
        private IDialogService dialogs;
        private MemberProfile memberProfile;

        [ImportingConstructor]
        public MemberViewModel(
            [Import(typeof(IAuthorizationService))] IAuthorizationService authorizator,
            [Import(typeof(IBackgroundExecutor))] IBackgroundExecutor executor,
            [Import(typeof(IEventAggregator))] IEventAggregator aggregator,
            [Import] IDialogService dialogs,
            [Import(typeof(ITasksService))] ITasksService taskService,
            [Import(typeof(ITeamService))] ITeamService teamService) {

            this.taskService = taskService;
            this.authorizator = authorizator;
            this.executor = executor;
            this.aggregator = aggregator;
            this.teamService = teamService;
            this.dialogs = dialogs;

            aggregator.Subscribe(ScrumFactoryEvent.ShowProfile, Show);
            aggregator.Subscribe<MemberProfile>(ScrumFactoryEvent.SignedMemberChanged, OnSignedMemberChanged);

            ChangeAvatarCommand = new DelegateCommand(ChangeMemberImage);
            RemoveAvatarCommand = new DelegateCommand(RemoveMemberImage);
            CloseWindowCommand = new DelegateCommand(Close);
            CreateAvatarCommand = new DelegateCommand(CreateAvatar);


            UpdateAvatarCommand = new DelegateCommand(() => {
                executor.StartBackgroundTask(
                () => {
                    teamService.UpdateMember(authorizator.SignedMemberProfile.MemberUId, MemberProfile);
                }, () => {
                    myProfileNocache = new Random().Next().ToString();
                    DefineMemberAvatarUrl();
                });
            });
        }

        public MemberViewModel(MemberProfile member, IServerUrl serverUrl, IAuthorizationService authorizator) {
            this.authorizator = authorizator;
            ServerUrl = serverUrl;
            MemberProfile = member;

            SendEmailCommand = new DelegateCommand(SendEmail);
        }

        private void OnSignedMemberChanged(MemberProfile member) {
            ShowIfProfileNotCompleted(member);
        }

        private void ShowIfProfileNotCompleted(MemberProfile member) {
            if (member == null)
                return;

            if (String.IsNullOrEmpty(member.EmailAccount)) {
                dialogs.SetBackTopMenu();
                Show();
            }
        }

        private new void Close() {
            executor.StartBackgroundTask(
                () => {
                    teamService.UpdateMember(authorizator.SignedMemberProfile.MemberUId, MemberProfile);
                }, () => { });
            dialogs.GoBackSelectedTopMenu();
        }

        public void Show() {
            MemberProfile = authorizator.SignedMemberProfile;
            dialogs.SelectTopMenu(this);
        }

        public MemberProfile MemberProfile {
            get {
                return memberProfile;
            }
            set {
                memberProfile = value;
                DefineMemberAvatarUrl();                
                OnPropertyChanged("MemberProfile");
            }
        }

 

        public void CreateAvatar() {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("http://www.gravatar.com"));
        }
     

      

        public ICommand CloseWindowCommand { get; private set; }

        public ICommand ChangeAvatarCommand { get; private set; }

        public ICommand RemoveAvatarCommand { get; private set; }

        public ICommand CreateAvatarCommand { get; private set; }

        public ICommand UpdateAvatarCommand { get; private set; }

        public ICommand SendEmailCommand { get; set; }



        /// <summary>
        /// Gets or sets a value indicating whether this instance is member selected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is member selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsMemberSelected {
            get {
                return isMemberSelected;
            }
            set {
                isMemberSelected = true;
                OnPropertyChanged("IsMemberSelected");
            }
        }

        [Import(typeof(MyProfile))]
        public IView View { get; set; }

        [Import(typeof(IServerUrl))]
        private IServerUrl ServerUrl { get; set; }

        private string myProfileNocache = "0";

        private void DefineMemberAvatarUrl() {
            DefineMemberAvatarUrl(MemberProfile);
            ScrumFactory.Windows.Helpers.Converters.MemberAvatarUrlConverter.ResetCache();
        }

        public void DefineMemberAvatarUrl(MemberProfile member) {
            if (member == null)
                return;

            if (authorizator == null || authorizator.SignedMemberProfile == null)
                return;

            if (member.MemberUId == authorizator.SignedMemberProfile.MemberUId)
                member.MemberAvatarUrl = ServerUrl.Url + "/MemberImage.aspx?MemberUId=" + member.MemberUId + "&nocache=" + myProfileNocache;
            else
                member.MemberAvatarUrl = ServerUrl.Url + "/MemberImage.aspx?MemberUId=" + member.MemberUId;


            OnPropertyChanged("MemberAvatarUrl");
        }

        public string MemberAvatarUrl {
            get {
                if (MemberProfile == null)
                    return null;
                return MemberProfile.MemberAvatarUrl;
            }
        }

        public bool IsContactMember {
            get {
                if (MemberProfile == null)
                    return false;
                return MemberProfile.IsContactMember;
            }
        }

        private void RemoveMemberImage() {
            executor.StartBackgroundTask(
                () => { teamService.RemoveMemberAvatar(MemberProfile.MemberUId); },
                () => {
                    myProfileNocache = new Random().Next().ToString();
                    DefineMemberAvatarUrl();
                });
        }

        private void ChangeMemberImage() {

            Byte[] imageBytes = GetMemberImageAsBytes();
            if (imageBytes == null)
                return;

            MemberAvatar avatar = new MemberAvatar();
            avatar.MemberUId = MemberProfile.MemberUId;
            avatar.AvatarImage = imageBytes;

            executor.StartBackgroundTask(
                () => { teamService.UpdateMemberAvatar(MemberProfile.MemberUId, avatar); },
                () => {
                    myProfileNocache = new Random().Next().ToString();
                    DefineMemberAvatarUrl();
                });
        }

        private Byte[] GetMemberImageAsBytes() {
            System.Drawing.Image image = null;
            Byte[] imageBytes = null;

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();            
            dialog.Filter = "Images|*.gif;*.jpg;*.png";

            bool? d = dialog.ShowDialog();
            if (d!=true)
                return null;
            try {
                image = System.Drawing.Bitmap.FromFile(dialog.FileName);
            } catch (Exception) {
                //Windows.Error.ShowAlert(Properties.Resources.Member_image_is_invalid);
            }
            if (image == null)
                return null;

            System.Drawing.Image imageResized = AutoResize(image);


            try {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                imageResized.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageBytes = ms.ToArray();
            } catch (Exception) {
                //Windows.Error.ShowAlert(Properties.Resources.Member_image_is_invalid);
            }

            return imageBytes;
        }


        private System.Drawing.Image AutoResize(System.Drawing.Image originalImg) {
            double resizeFactor = 1;
            double MAX_DIMENSION = 100;
            int newWidth = originalImg.Width;
            int newHeight = originalImg.Height;


            // if image is bigger then MAX_DIMENSION x MAX_DIMENSION
            // calculates the new dimensiom
            if (originalImg.Width > MAX_DIMENSION || originalImg.Height > MAX_DIMENSION) {

                // use the lowest dimension to calculate the resize factor
                if (originalImg.Width < originalImg.Height)
                    resizeFactor = (double)(MAX_DIMENSION / originalImg.Width);
                else
                    resizeFactor = (double)(MAX_DIMENSION / originalImg.Height);

                newWidth = (int)(originalImg.Width * resizeFactor);
                newHeight = (int)(originalImg.Height * resizeFactor);

            }

            // Resize the image                                                
            System.Drawing.Bitmap newImg = new System.Drawing.Bitmap(originalImg, newWidth, newHeight);


            // Now crop it
            int m;
            System.Drawing.Rectangle crop;
            if (newImg.Width < newImg.Height) {
                m = (newImg.Height - newImg.Width) / 2;
                crop = new System.Drawing.Rectangle(0, m, newImg.Width, newImg.Width);
            } else {
                m = (newImg.Width - newImg.Height) / 2;
                crop = new System.Drawing.Rectangle(m, 0, newImg.Height, newImg.Height);
            }

            return newImg.Clone(crop, newImg.PixelFormat);
        }

        public override string ToString() {
            if (MemberProfile == null)
                return String.Empty;
            return MemberProfile.FullName;
        }

        protected override void OnDispose() {

            if (aggregator != null)
                aggregator.UnSubscribeAll(this);

            ChangeAvatarCommand = null; OnPropertyChanged("ChangeAvatarCommand");
            RemoveAvatarCommand = null; OnPropertyChanged("RemoveAvatarCommand");
            CloseWindowCommand = null; OnPropertyChanged("CloseWindowCommand");
            CreateAvatarCommand = null; OnPropertyChanged("CreateAvatarCommand");
            SendEmailCommand = null; OnPropertyChanged("SendEmailCommand");
            UpdateAvatarCommand = null; OnPropertyChanged("UpdateAvatarCommand");

        }

        ~MemberViewModel() {
            System.Console.Out.WriteLine("***< member died here");
        }

        private void SendEmail() {
            if (MemberProfile == null)
                return;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("mailto:" + MemberProfile.EmailAccount));
        }


        public decimal? PlannedHoursForToday {
            get {
                if (MemberProfile == null)
                    return null;
                return MemberProfile.PlannedHoursForToday;
            }
            set {
                if (MemberProfile == null)
                    return;
                MemberProfile.PlannedHoursForToday = value;
                OnPropertyChanged("IsTodayHalfPlanned");
                OnPropertyChanged("IsTodayOverPlanned");
            }
        }

        public bool IsTodayHalfPlanned {
            get {
                if (MemberProfile == null)
                    return false;
                return MemberProfile.IsTodayHalfPlanned;
            }
        }

        public bool IsTodayOverPlanned {
            get {
                if (MemberProfile == null)
                    return false;
                return MemberProfile.IsTodayOverPlanned;
            }
        }

       

        public bool FilterTest(string filter) {
            if (filter == null)
                return true;
            string t = filter.ToLower();

            string teamCodePrefix = GetTeamCodePrefix(t);

            if (MemberProfile.FullName.ToLower().Contains(t))
                return true;
            if (MemberProfile.EmailAccount != null && MemberProfile.EmailAccount.ToLower().Contains(t))
                return true;
            if (MemberProfile.Skills != null && MemberProfile.Skills.ToLower().Contains(t))
                return true;
            if (MemberProfile.TeamCode != null && MemberProfile.TeamCode.ToLower() == t)
                return true;
            if (MemberProfile.TeamCode != null && t == teamCodePrefix)
                return true;
            return false;
        }

        private string GetTeamCodePrefix(string filter) {
            int idx = filter.IndexOf(".");
            if (idx < 2)
                return String.Empty;
            return filter.Substring(0, idx - 1);
        }

        #region IPanelViewModel Members

        public string PanelName {
            get { return Properties.Resources.My_profile; }
        }

        public int PanelDisplayOrder {
            get { return int.MaxValue; }
        }



        #endregion


        public PanelPlacements PanelPlacement {
            get { return PanelPlacements.Hidden; }
        }

        public string ImageUrl {
            get { return null; }
        }
    }
}
