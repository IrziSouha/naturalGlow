using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace naturalGlow.Auth
{
    public class Autentication
    {
        private static FirebaseApp firebaseApp = InitializeFirebase();

        private static string apiKey =
            "AIzaSyC6qeBJF6B_Keu1My8DlEs_amY5dUgb7LM";

        public static string ApiKey => apiKey;


        public static FirebaseApp InitializeFirebase()
        {
            var credential = GoogleCredential.FromFile(@"C:\Treball Final\naturalGlow\naturalGlow\naturalGlow\natural.json");

            return FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });
        }


        #region SignIn

        public static async Task<FirebaseAuthLink> SignIn(
            string email,
            string password)
        {
            try
            {
                var authProvider =
                    new FirebaseAuthProvider(
                        new FirebaseConfig(apiKey));

                FirebaseAuthLink auth =
                    await authProvider.SignInWithEmailAndPasswordAsync(
                        email,
                        password);

                return auth;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al fer login: " + ex.Message);

                return null;
            }
        }

        #endregion

        #region SignUp

        public static async Task<FirebaseAuthLink> SignUp(
            string email,
            string password,
            string alias)
        {
            try
            {
                var authProvider =
                    new FirebaseAuthProvider(
                        new FirebaseConfig(apiKey));

                FirebaseAuthLink auth =
                    await authProvider.CreateUserWithEmailAndPasswordAsync(
                        email,
                        password,
                        alias);

                return auth;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en el registre: " + ex.Message);

                return null;
            }
        }

        #endregion

        #region Recuperar Contrasenya

        public static async Task<bool> RecuperarContrasenya(
            string email)
        {
            try
            {
                var authProvider =
                    new FirebaseAuthProvider(
                        new FirebaseConfig(apiKey));

                await authProvider.SendPasswordResetEmailAsync(
                    email);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error en recuperar la contrasenya:\n\n"
                    + ex.Message);

                return false;
            }
        }

        #endregion

        #region Canviar Contrasenya

        public static async Task<bool> CanviarContrasenya(
            FirebaseAuthLink auth,
            string novaContrasenya)
        {
            try
            {
                // Comprovar que l'usuari està autenticat
                if (auth == null || auth.User == null)
                {
                    MessageBox.Show(
                        "La sessió de l'usuari no és vàlida.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }

                // Comprovar que s'ha introduït una contrasenya
                if (string.IsNullOrWhiteSpace(novaContrasenya))
                {
                    MessageBox.Show(
                        "Introdueix una nova contrasenya.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }

                // Comprovar la longitud mínima
                if (novaContrasenya.Length < 6)
                {
                    MessageBox.Show(
                        "La contrasenya ha de tenir almenys 6 caràcters.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return false;
                }

                // Obtenir l'identificador de l'usuari autenticat
                string uid = auth.User.LocalId;

                // Crear les dades d'actualització
                UserRecordArgs args = new UserRecordArgs
                {
                    Uid = uid,
                    Password = novaContrasenya
                };

                await FirebaseAdmin.Auth.FirebaseAuth
                    .DefaultInstance
                    .UpdateUserAsync(args);

                MessageBox.Show(
                    "La contrasenya s'ha canviat correctament.",
                    "Contrasenya actualitzada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No s'ha pogut canviar la contrasenya:\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

        #endregion
    }
}
