using Google.Cloud.Firestore;
using System;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class Proveidor
    {
        public Proveidor()
        {
            Id = "-";
            NomEmpresa = "";
            Contacte = "";
            Nif = "";
            Adreca = "";
            Telefon = "";
            Email = "";
            FormaPagament = "";
        }

        public Proveidor(
            string id,
            string nomEmpresa,
            string contacte,
            string nif,
            string adreca,
            string telefon,
            string email,
            string formaPagament)
        {
            Id = id;
            NomEmpresa = nomEmpresa;
            Contacte = contacte;
            Nif = nif;
            Adreca = adreca;
            Telefon = telefon;
            Email = email;
            FormaPagament = formaPagament;
        }

        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string NomEmpresa { get; set; }

        [FirestoreProperty]
        public string Contacte { get; set; }

        [FirestoreProperty]
        public string Nif { get; set; }

        [FirestoreProperty]
        public string Adreca { get; set; }

        [FirestoreProperty]
        public string Telefon { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string FormaPagament { get; set; }

        public override string ToString()
        {
            return $"{NomEmpresa} - {Nif} - {Telefon}";
        }
    }
}
