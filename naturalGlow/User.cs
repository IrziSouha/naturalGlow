using Firebase.Auth;
using Google.Cloud.Firestore;
using System;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class User
    {
        public User()
        {
            Id = "-";
            Nom = "";
            Email = "";
            Password = "";
            Rol = "";
            Telefon = "";
            Adreca = "";
        }

        public User(
            string id,
            string nom,
            string email,
            string password,
            string rol,
            string telefon,
            string adreca)
        {
            Id = id;
            Nom = nom;
            Email = email;
            Password = password;
            Rol = rol;
            Telefon = telefon;
            Adreca = adreca;
        }

        [FirestoreProperty]
        public string Id { get; set; }


        [FirestoreProperty]
        public string Nom { get; set; }


        [FirestoreProperty]
        public string Email { get; set; }


        [FirestoreProperty]
        public string Password { get; set; }


        [FirestoreProperty]
        public string Rol { get; set; }


    
        [FirestoreProperty]
        public string Telefon { get; set; }


      
        [FirestoreProperty]
        public string Adreca { get; set; }


        public override string ToString()
        {
            return $"{Nom} - {Email} - {Rol}";
        }


       
    }
}