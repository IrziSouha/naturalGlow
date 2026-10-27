using Google.Cloud.Firestore;
using System;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class Producte
    {
        public Producte()
        {
            Id = "-";
            Nom = "";
            Descripcio = "";
            Categoria = "";
            PreuCompra = 0;
            PreuVenda = 0;
            Stock = 0;
            StockMinim = 0;
            ImatgeURL = "";
        }

        public Producte(
            string id,
            string nom,
            string descripcio,
            string categoria,
            double preuCompra,
            double preuVenda,
            int stock,
            int stockMinim,
            string imatgeURL)
        {
            Id = id;
            Nom = nom;
            Descripcio = descripcio;
            Categoria = categoria;
            PreuCompra = preuCompra;
            PreuVenda = preuVenda;
            Stock = stock;
            StockMinim = stockMinim;
            ImatgeURL = imatgeURL;
        }


        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Nom { get; set; }

        [FirestoreProperty]
        public string Descripcio { get; set; }

        [FirestoreProperty]
        public string Categoria { get; set; }

        [FirestoreProperty]
        public double PreuCompra { get; set; }

        [FirestoreProperty]
        public double PreuVenda { get; set; }

        [FirestoreProperty]
        public int Stock { get; set; }

        [FirestoreProperty]
        public int StockMinim { get; set; }

        [FirestoreProperty]
        public string ImatgeURL { get; set; }

        public string Estat
        {
            get
            {
                bool trobat = true;
                string estat = "";

                if (Stock <= 0)
                {
                    estat = "Esgotat";
                }
                else if (Stock <= StockMinim)
                {
                    estat = "Stock baix";
                }
                else
                {
                    estat = "Disponible";
                }

                return estat;
            }
        }

        public override string ToString()
        {
            return $"{Nom} - {Categoria} - {PreuVenda}€";
        }
    }
}