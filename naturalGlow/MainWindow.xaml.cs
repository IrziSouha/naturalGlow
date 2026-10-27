using Firebase.Auth;
using naturalGlow.Auth;
using naturalGlow.DAO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserModel = naturalGlow.MODEL.User;
namespace naturalGlow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FirebaseAuthProvider authProvider;
        private IDAO dao;
        public MainWindow()
        {
            InitializeComponent();
            authProvider = new FirebaseAuthProvider(
                new FirebaseConfig("AIzaSyC6qeBJF6B_Keu1My8DlEs_amY5dUgb7LM"));

            dao = new IDAOFirebaseImpl(
            @"C:\Treball Final\naturalGlow\naturalGlow\naturalGlow\natural.json",
            "naturalglow");
        }

        #region Registrar Venedor

        private async void BtnRegistrarVenedor_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string email =
                    txtEmailVenedor.Text.Trim();

                string password =
                    txtPasswordVenedor.Password;

                string confirmPassword =
                    txtConfirmPasswordVenedor.Password;


                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show(
                        "Has d'introduir un correu electrònic.");

                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(
                        "Has d'introduir una contrasenya.");

                    return;
                }

                if (password != confirmPassword)
                {
                    MessageBox.Show(
                        "Les contrasenyes no coincideixen.");

                    return;
                }

                FirebaseAuthLink auth =
                    await Autentication.SignUp(
                        email,
                        password,
                        email);

                if (auth == null)
                {
                    MessageBox.Show(
                        "No s'ha pogut crear l'usuari.");

                    return;
                }

                string id =
                    auth.User.LocalId;


                UserModel nouVenedor =
                    new UserModel(
                        id,
                        "",
                        email,
                        "",
                        "venedor",
                        "",
                        "");

                await dao.AddUser(nouVenedor);


                MessageBox.Show(
                    "El venedor s'ha registrat correctament.");

                PantallaVenedor pantallaVenedor =
                    new PantallaVenedor();

                pantallaVenedor.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en registrar el venedor:\n\n"
                    + ex.Message);
            }
        }

        #endregion

        #region Registrar Client

        private async void BtnRegistrarClient_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string email =
                    txtEmailClient.Text.Trim();

                string password =
                    txtPasswordClient.Password;

                string confirmPassword =
                    txtConfirmPasswordClient.Password;


                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show(
                        "Has d'introduir un correu electrònic.");

                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(
                        "Has d'introduir una contrasenya.");

                    return;
                }

                if (password != confirmPassword)
                {
                    MessageBox.Show(
                        "Les contrasenyes no coincideixen.");

                    return;
                }

                FirebaseAuthLink auth =
                    await Autentication.SignUp(
                        email,
                        password,
                        email);

                if (auth == null)
                {
                    MessageBox.Show(
                        "No s'ha pogut crear l'usuari.");

                    return;
                }


                string id =
                    auth.User.LocalId;


                UserModel nouClient =
                    new UserModel(
                        id,
                        "",
                        email,
                        "",
                        "client",
                        "",
                        "");


                await dao.AddUser(nouClient);

                MessageBox.Show(
                    "El client s'ha registrat correctament.");


                txtEmailClient.Clear();

                txtPasswordClient.Clear();

                txtConfirmPasswordClient.Clear();

                PantallaClient pantallaClient =
                        new PantallaClient(id);

                pantallaClient.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en registrar el client:\n\n"
                    + ex.Message);
            }
        }


        #endregion
       
        #region Login Venedor

        private async void BtnLoginVenedor_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string email =
                    txtEmailLoginVenedor.Text.Trim();


                string password =
                    txtPasswordLoginVenedor.Password;


                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show(
                        "Has d'introduir el correu electrònic.");

                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(
                        "Has d'introduir la contrasenya.");

                    return;
                }


                FirebaseAuthLink auth =
                    await Autentication.SignIn(
                        email,
                        password);


                if (auth == null)
                {
                    MessageBox.Show(
                        "No tens cap compte o el correu o la contrasenya no són correctes.\n\n" +
                        "Si encara no tens un compte, has de registrar-te.");

                    return;
                }

                string id =
                    auth.User.LocalId;


                UserModel usuari =
                    await dao.GetUserById(id);

                if (usuari == null)
                {
                    MessageBox.Show(
                        "Aquest compte no està registrat com a venedor.\n\n" +
                        "Has de registrar-te com a venedor.");

                    return;
                }

                if (usuari.Rol != "venedor")
                {
                    MessageBox.Show(
                        "Aquest compte no és un compte de venedor.");

                    return;
                }

                MessageBox.Show(
                    "Has iniciat sessió correctament.");

                PantallaVenedor pantallaVenedor =
                    new PantallaVenedor();

                pantallaVenedor.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al iniciar sessió:\n\n"
                    + ex.Message);
            }
        }

        #endregion

        #region Recuperar Contrasenya Venedor

        private async void BtnRecuperarPasswordVenedor_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string email =
                    txtEmailLoginVenedor.Text.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show(
                        "Has d'introduir el teu correu electrònic " +
                        "per poder recuperar la contrasenya.");

                    return;
                }


                bool resultat =
                    await Autentication.RecuperarContrasenya(
                        email);

                if (resultat)
                {
                    MessageBox.Show(
                        "S'ha enviat un correu per recuperar " +
                        "la teva contrasenya.\n\n" +
                        "Revisa la teva safata d'entrada i segueix " +
                        "les instruccions del correu.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al recuperar la contrasenya:\n\n"
                    + ex.Message);
            }
        }

        #endregion

        #region Login Client

        private async void BtnLoginClient_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string email =
                    txtEmailLoginClient.Text.Trim();

                string password =
                    txtPasswordLoginClient.Password;


                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show(
                        "Has d'introduir el correu electrònic.");

                    return;
                }


                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(
                        "Has d'introduir la contrasenya.");

                    return;
                }

                FirebaseAuthLink auth =
                    await Autentication.SignIn(
                        email,
                        password);


                if (auth == null)
                {
                    MessageBox.Show(
                        "No tens cap compte o el correu "
                        + "o la contrasenya no són correctes.");

                    return;
                }

                string id =
                    auth.User.LocalId;

                UserModel usuari =
                    await dao.GetUserById(id);

                if (usuari == null)
                {
                    MessageBox.Show(
                        "Aquest compte no està registrat com a client.");

                    return;
                }


                if (usuari.Rol != "client")
                {
                    MessageBox.Show(
                        "Aquest compte no és un compte de client.");

                    return;
                }


                MessageBox.Show(
                    "Has iniciat sessió correctament.");

                PantallaClient pantallaClient =
                    new PantallaClient(id);

                pantallaClient.Show();


                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al iniciar sessió:\n\n"
                    + ex.Message);
            }
        }

        #endregion

        #region Recuperar Contrasenya Client

        private async void BtnRecuperarPasswordClient_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                string email =
                    txtEmailLoginClient.Text.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show(
                        "Has d'introduir el teu correu electrònic " +
                        "per poder recuperar la contrasenya.");

                    return;
                }

                bool resultat =
                    await Autentication.RecuperarContrasenya(
                        email);

                if (resultat)
                {
                    MessageBox.Show(
                        "S'ha enviat un correu per recuperar " +
                        "la teva contrasenya.\n\n" +
                        "Revisa la teva safata d'entrada i segueix " +
                        "les instruccions del correu.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al recuperar la contrasenya:\n\n"
                    + ex.Message);
            }
        }

        #endregion

    }
}