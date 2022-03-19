using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Egelke.Eid.Client;
using Egelke.Eid.Client.Model;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace EidReaderApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Readers readers;
        System.Drawing.Image currentPicture;
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public MainWindow()
        {
            InitializeComponent();
            this.WindowOnLoad(this, EventArgs.Empty);
        }

        private void RestartApp(object sender, EventArgs e)
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location.Split(".dll")[0] + ".exe";
            Process.Start(exePath);
            this.Close();
        }

        private System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            return returnImage;
        }

        private BitmapImage ImageFromBuffer(Byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        private void ReadingCard(object sender, EventArgs e)
        {
            EidCard eid = (EidCard)readers.ListCards().Where(c => c is EidCard).FirstOrDefault();
            using (eid)
            {
                eid.Open();
                Identity identity = eid.Identity;
                Address address = eid.Address;

                identityPicture.Source = ImageFromBuffer(eid.Picture);
                currentPicture = byteArrayToImage(eid.Picture);

                lbl_firstname.Content = $"Prénoms: {identity.FirstNames}";
                lbl_lastname.Content = $"Nom: {identity.Surname}";
                lbl_birthday.Content = $"Date de naissance: {identity.DateOfBirth.Day}/{identity.DateOfBirth.Month}/{identity.DateOfBirth.Year}";
                lbl_location_birth.Content = $"Lieu de naissance: {identity.LocationOfBirth}";
                lbl_gender.Content = $"Genre: {identity.Gender}";
                lbl_national_number.Content = $"Numéro national: {identity.NationalNr}";
                lbl_nationality.Content = $"Nationalité: {identity.Nationality}";
                lbl_document_type.Content = $"Type de docuement: {identity.DocumentType}";
                lbl_card_number.Content = $"Numéro de carte: {identity.CardNr}";
                lbl_address.Content = $"Adresse: {address.StreetAndNumber}, {address.Zip}, {address.Municipality}";
                lbl_municipality.Content = $"Lieu d'émission: {identity.IssuingMunicipality}";
                lbl_valid_start.Content = $"Début de la validité: {identity.ValidityBeginDate.Day}/{identity.ValidityBeginDate.Month}/{identity.ValidityBeginDate.Year}";
                lbl_valid_end.Content = $"Fin de la validité: {identity.ValidityEndDate.Day}/{identity.ValidityEndDate.Month}/{identity.ValidityEndDate.Year}";

            }
        }


        private void SaveIdentityPicture(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = desktopPath;
            sfd.Title = "Sauvegarde de la photo de la carte...";
            sfd.Filter = "Image PNG (*.png)|*.png|Image JPG (*.jpg)|*.jpg|Image JPEG (*.jpeg)|*.jpeg";
            sfd.FilterIndex = 0;
            if (sfd.ShowDialog() == true)
            {
                if (sfd.FileName != "")
                {
                    string pathToSave = sfd.FileName;
                    currentPicture.Save(pathToSave);
                    MessageBoxResult result = MessageBox.Show("Voulez vous ouvrir le dossier de l'image ?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if(result == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", $@"{System.IO.Path.GetDirectoryName(pathToSave)}");
                    }
                }
            }
        }

        private void WindowOnLoad(object sender, EventArgs e)
        {
            try
            {
                readers = new Readers(ReaderScope.User);
                readers.CardInsert += ReadingCard;
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show($"Une erreur s'est produite avec le lecteur\nError : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                if(result == MessageBoxResult.OK)
                {
                    this.RestartApp(this, EventArgs.Empty);
                }
            }
        }

        private void ReadEidCard(object sender, EventArgs e)
        {
            this.ReadingCard(this, EventArgs.Empty);
        }
    }
}
