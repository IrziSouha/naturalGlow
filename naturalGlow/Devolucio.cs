using Google.Cloud.Firestore;
using System;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class Devolucio
    {
        public Devolucio()
        {
            Id = "-";
            IdComanda = "";
            IdProducte = "";
            Quantitat = 0;
            Data = DateTime.Now;
            Estat = "";
        }

        public Devolucio(
            string id,
            string idComanda,
            string idProducte,
            int quantitat,
            DateTime data,
            string estat)
        {
            Id = id;
            IdComanda = idComanda;
            IdProducte = idProducte;
            Quantitat = quantitat;
            Data = data;
            Estat = estat;
        }

        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string IdComanda { get; set; }

        [FirestoreProperty]
        public string IdProducte { get; set; }

        [FirestoreProperty]
        public int Quantitat { get; set; }

        [FirestoreProperty]
        public DateTime Data { get; set; }

        [FirestoreProperty]
        public string Estat { get; set; }

        public override string ToString()
        {
            return $"Devolució {Id} - Comanda: {IdComanda} - Producte: {IdProducte} - Quantitat: {Quantitat}";
        }
    }
}