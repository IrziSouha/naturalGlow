using Google.Cloud.Firestore;
using System;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class Factura
    {
        public Factura()
        {
            Id = "-";
            IdComanda = "";
            Data = DateTime.UtcNow;
            Total = 0;
            PdfURL = "";
        }

        public Factura(
            string id,
            string idComanda,
            DateTime data,
            double total,
            string pdfURL)
        {
            Id = id;
            IdComanda = idComanda;
            Data = data;
            Total = total;
            PdfURL = pdfURL;
        }


        [FirestoreProperty]
        public string Id { get; set; }


        [FirestoreProperty]
        public string IdComanda { get; set; }


        [FirestoreProperty]
        public DateTime Data { get; set; }


        [FirestoreProperty]
        public double Total { get; set; }


        [FirestoreProperty]
        public string PdfURL { get; set; }


        public override string ToString()
        {
            return $"{Id} - Comanda: {IdComanda} - {Total:F2}€";
        }
    }
}