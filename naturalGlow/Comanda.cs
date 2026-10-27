using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace naturalGlow.MODEL
{
    [FirestoreData]
    public class Comanda
    {
        public Comanda()
        {
            Id = "-";
            IdClient = "";
            Data = DateTime.UtcNow;
            Total = 0;
            Estat = "En preparació";
            LlistaProductes = new List<LiniaComanda>();
        }


        public Comanda(
            string id,
            string idClient,
            DateTime data,
            double total,
            string estat,
            List<LiniaComanda> llistaProductes)
        {
            Id = id;
            IdClient = idClient;

            if (data.Kind == DateTimeKind.Unspecified)
            {
                data = DateTime.SpecifyKind(
                    data,
                    DateTimeKind.Utc);
            }
            else if (data.Kind == DateTimeKind.Local)
            {
                data = data.ToUniversalTime();
            }

            Data = data;

            Total = total;
            Estat = estat;
            LlistaProductes = llistaProductes;
        }


        [FirestoreProperty]
        public string Id { get; set; }


        [FirestoreProperty]
        public string IdClient { get; set; }


        [FirestoreProperty]
        public DateTime Data { get; set; }


        [FirestoreProperty]
        public double Total { get; set; }


        [FirestoreProperty]
        public string Estat { get; set; }


        [FirestoreProperty]
        public List<LiniaComanda> LlistaProductes { get; set; }


        public override string ToString()
        {
            return $"{Id} - {Data.ToLocalTime()} - {Total:F2}€ - {Estat}";
        }
    }
}