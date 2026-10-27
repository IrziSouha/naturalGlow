using Google.Cloud.Firestore;
using naturalGlow.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace naturalGlow.DAO
{
    public class IDAOFirebaseImpl : IDAO
    {
        #region VARIABLES

        FirestoreDb db;
        private string token;
        private string project;

        #endregion

        #region CONSTRUCTOR

        public IDAOFirebaseImpl(
            string token,
            string projectName)
        {
            this.token = token;
            this.project = projectName;

            SetCredentials();

            db = FirestoreDb.Create(projectName);
        }

        #endregion

        #region CREDENCIALS FIREBASE

        private void SetCredentials()
        {
            string pathToServiceAccount =
                @"C:\Treball Final\naturalGlow\naturalGlow\naturalGlow\natural.json";

            Environment.SetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS",
                pathToServiceAccount);
        }

        #endregion

        #region USERS
        public async Task AddUser(User user)
        {
            DocumentReference documentReference =
                db.Collection("users")
                  .Document(user.Id);

            await documentReference.SetAsync(user);
        }

        public async Task DeleteUser(User user)
        {
            DocumentReference documentReference =
                db.Collection("users")
                  .Document(user.Id);

            await documentReference.DeleteAsync();
        }

        public async Task<List<User>> GetAllUsers()
        {
            Query query =
                db.Collection("users");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<User> users =
                new List<User>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    User user =
                        document.ConvertTo<User>();

                    users.Add(user);
                }
            }

            return users;
        }

        public async Task<User> GetUserByEmail(
            string email)
        {
            Query query =
                db.Collection("users")
                  .WhereEqualTo(
                      "Email",
                      email);

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            if (snapshot.Count == 0)
            {
                return null;
            }

            DocumentSnapshot document =
                snapshot.Documents[0];

            return document.ConvertTo<User>();
        }

        public async Task<User> GetUserById(
            string id)
        {
            DocumentReference documentReference =
                db.Collection("users")
                  .Document(id);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<User>();
        }

        public async Task UpdateUser(User user)
        {
            DocumentReference documentReference =
                db.Collection("users")
                  .Document(user.Id);

            await documentReference.SetAsync(
                user,
                SetOptions.MergeAll);
        }


        #endregion

        #region PRODUCTES

        public async Task AddProducte(
            Producte producte)
        {
            DocumentReference documentReference =
                db.Collection("productes")
                  .Document(producte.Id);

            await documentReference.SetAsync(
                producte);
        }

        public async Task UpdateProducte(
            Producte producte)
        {
            DocumentReference documentReference =
                db.Collection("productes")
                  .Document(producte.Id);

            await documentReference.SetAsync(
                producte,
                SetOptions.MergeAll);
        }

        public async Task<Producte> GetProducteById(
            string id)
        {
            DocumentReference documentReference =
                db.Collection("productes")
                  .Document(id);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<Producte>();
        }

        public async Task<List<Producte>> GetAllProductes()
        {
            Query query =
                db.Collection("productes");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Producte> productes =
                new List<Producte>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Producte producte =
                        document.ConvertTo<Producte>();

                    productes.Add(producte);
                }
            }

            return productes;
        }

        public async Task<List<Producte>>
            GetProductesByCategoria(
                string categoria)
        {
            Query query =
                db.Collection("productes")
                  .WhereEqualTo(
                      "Categoria",
                      categoria);

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Producte> productes =
                new List<Producte>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Producte producte =
                        document.ConvertTo<Producte>();

                    productes.Add(producte);
                }
            }

            return productes;
        }

        public async Task<List<Producte>>
            GetProductesWithStockBaix()
        {
            List<Producte> productes =
                await GetAllProductes();

            return productes
                .Where(p =>
                    p.Stock <= p.StockMinim)
                .ToList();
        }
        public async Task DeleteProducte(
            Producte producte)
        {
            DocumentReference documentReference =
                db.Collection("productes")
                  .Document(producte.Id);

            await documentReference.DeleteAsync();
        }


        #endregion

        #region COMANDES
        public async Task AddComanda(
            Comanda comanda)
        {
            DocumentReference documentReference =
                db.Collection("comandes")
                  .Document(comanda.Id);

            await documentReference.SetAsync(
                comanda);
        }
        public async Task UpdateComanda(
            Comanda comanda)
        {
            DocumentReference documentReference =
                db.Collection("comandes")
                  .Document(comanda.Id);

            await documentReference.SetAsync(
                comanda,
                SetOptions.MergeAll);
        }

        public async Task<Comanda> GetComandaById(
            string id)
        {
            DocumentReference documentReference =
                db.Collection("comandes")
                  .Document(id);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<Comanda>();
        }

        public async Task<List<Comanda>>
            GetAllComandes()
        {
            Query query =
                db.Collection("comandes");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Comanda> comandes =
                new List<Comanda>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Comanda comanda =
                        document.ConvertTo<Comanda>();

                    comandes.Add(comanda);
                }
            }

            return comandes;
        }


        public async Task<List<Comanda>>GetComandesByClient(string idClient)
        {
            Query query =
                db.Collection("comandes")
                  .WhereEqualTo(
                      "IdClient",
                      idClient);

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Comanda> comandes =
                new List<Comanda>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Comanda comanda =
                        document.ConvertTo<Comanda>();

                    comandes.Add(comanda);
                }
            }

            return comandes;
        }


        public async Task DeleteComanda(
            Comanda comanda)
        {
            DocumentReference documentReference =
                db.Collection("comandes")
                  .Document(comanda.Id);

            await documentReference.DeleteAsync();
        }

        #endregion

        #region LINIA COMANDA
        public async Task AddLiniaComanda(
            LiniaComanda liniaComanda)
        {
            string idLinia =
                Guid.NewGuid().ToString();

            DocumentReference documentReference =
                db.Collection("liniesComanda")
                  .Document(idLinia);

            await documentReference.SetAsync(
                liniaComanda);
        }
        public async Task UpdateLiniaComanda(
            LiniaComanda liniaComanda)
        {
            DocumentReference documentReference =
                db.Collection("liniesComanda")
                  .Document(liniaComanda.IdProducte);

            await documentReference.SetAsync(
                liniaComanda,
                SetOptions.MergeAll);
        }


        public async Task<LiniaComanda>
            GetLiniaComandaById(
                string idProducte)
        {
            DocumentReference documentReference =
                db.Collection("liniesComanda")
                  .Document(idProducte);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<LiniaComanda>();
        }


        public async Task<List<LiniaComanda>>
            GetAllLiniesComanda()
        {
            Query query =
                db.Collection("liniesComanda");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<LiniaComanda> liniesComanda =
                new List<LiniaComanda>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    LiniaComanda liniaComanda =
                        document.ConvertTo<LiniaComanda>();

                    liniesComanda.Add(
                        liniaComanda);
                }
            }

            return liniesComanda;
        }

        public async Task DeleteLiniaComanda(
            LiniaComanda liniaComanda)
        {
            DocumentReference documentReference =
                db.Collection("liniesComanda")
                  .Document(liniaComanda.IdProducte);

            await documentReference.DeleteAsync();
        }

        #endregion

        #region FACTURES
        public async Task AddFactura(
         Factura factura)
        {
            if (factura == null)
            {
                throw new ArgumentNullException(
                    nameof(factura));
            }

            if (string.IsNullOrWhiteSpace(factura.Id))
            {
                throw new Exception(
                    "La factura no té un ID.");
            }

            DocumentReference documentReference =
                db.Collection("factures")
                  .Document(factura.Id);

            await documentReference.SetAsync(
                factura);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                throw new Exception(
                    "La factura no s'ha pogut guardar a Firestore.");
            }
        }
        public async Task UpdateFactura(
            Factura factura)
        {
            DocumentReference documentReference =
                db.Collection("factures")
                  .Document(factura.Id);

            await documentReference.SetAsync(
                factura,
                SetOptions.MergeAll);
        }

        public async Task<Factura> GetFacturaById(
            string id)
        {
            DocumentReference documentReference =
                db.Collection("factures")
                  .Document(id);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<Factura>();
        }

        public async Task<List<Factura>>
            GetAllFactures()
        {
            Query query =
                db.Collection("factures");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Factura> factures =
                new List<Factura>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Factura factura =
                        document.ConvertTo<Factura>();

                    factures.Add(factura);
                }
            }

            return factures;
        }

        public async Task<Factura>
            GetFacturaByComanda(
                string idComanda)
        {
            Query query =
                db.Collection("factures")
                  .WhereEqualTo(
                      "IdComanda",
                      idComanda);

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            if (snapshot.Count == 0)
            {
                return null;
            }

            DocumentSnapshot document =
                snapshot.Documents[0];

            return document.ConvertTo<Factura>();
        }

        public async Task DeleteFactura(
            Factura factura)
        {
            DocumentReference documentReference =
                db.Collection("factures")
                  .Document(factura.Id);

            await documentReference.DeleteAsync();
        }

        #endregion

        #region PROVEIDORS
        public async Task AddProveidor(Proveidor proveidor)
        {
            DocumentReference documentReference =
                db.Collection("proveidors")
                  .Document(proveidor.Id);

            await documentReference.SetAsync(proveidor);
        }

        public async Task UpdateProveidor(Proveidor proveidor)
        {
            DocumentReference documentReference =
                db.Collection("proveidors")
                  .Document(proveidor.Id);

            await documentReference.SetAsync(
                proveidor,
                SetOptions.MergeAll);
        }

        public async Task<Proveidor> GetProveidorById(string id)
        {
            DocumentReference documentReference =
                db.Collection("proveidors")
                  .Document(id);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<Proveidor>();
        }

        public async Task<List<Proveidor>> GetAllProveidors()
        {
            Query query =
                db.Collection("proveidors");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Proveidor> proveidors =
                new List<Proveidor>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Proveidor proveidor =
                        document.ConvertTo<Proveidor>();

                    proveidors.Add(proveidor);
                }
            }

            return proveidors;
        }

        public async Task DeleteProveidor(Proveidor proveidor)
        {
            DocumentReference documentReference =
                db.Collection("proveidors")
                  .Document(proveidor.Id);

            await documentReference.DeleteAsync();
        }

        #endregion

        #region FACTURA PROVEIDOR
        public async Task AddFacturaProveidor(
            FacturaProveidor facturaProveidor)
        {
            if (facturaProveidor == null)
            {
                throw new ArgumentNullException(
                    nameof(facturaProveidor));
            }

            if (string.IsNullOrWhiteSpace(
                facturaProveidor.Id))
            {
                throw new Exception(
                    "La factura del proveïdor no té un ID.");
            }

            if (string.IsNullOrWhiteSpace(
                facturaProveidor.IdProveidor))
            {
                throw new Exception(
                    "La factura del proveïdor no té un proveïdor.");
            }

            if (facturaProveidor.LlistaProductes == null)
            {
                facturaProveidor.LlistaProductes =
                    new List<LiniaFacturaProveidor>();
            }

            DocumentReference documentReference =
                db.Collection("facturesProveidor")
                  .Document(facturaProveidor.Id);

            await documentReference.SetAsync(
                facturaProveidor);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                throw new Exception(
                    "La factura del proveïdor no s'ha pogut guardar a Firestore.");
            }
        }


        public async Task UpdateFacturaProveidor(
            FacturaProveidor facturaProveidor)
        {
            if (facturaProveidor == null)
            {
                throw new ArgumentNullException(
                    nameof(facturaProveidor));
            }

            DocumentReference documentReference =
                db.Collection("facturesProveidor")
                  .Document(facturaProveidor.Id);

            await documentReference.SetAsync(
                facturaProveidor,
                SetOptions.MergeAll);
        }


        public async Task<FacturaProveidor>
            GetFacturaProveidorById(
                string id)
        {
            DocumentReference documentReference =
                db.Collection("facturesProveidor")
                  .Document(id);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<FacturaProveidor>();
        }


        public async Task<List<FacturaProveidor>>
            GetAllFacturesProveidor()
        {
            Query query =
                db.Collection("facturesProveidor");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<FacturaProveidor> facturesProveidor =
                new List<FacturaProveidor>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    FacturaProveidor facturaProveidor =
                        document.ConvertTo<FacturaProveidor>();

                    facturesProveidor.Add(
                        facturaProveidor);
                }
            }

            return facturesProveidor;
        }


        public async Task<List<FacturaProveidor>>
            GetFacturesProveidorByProveidor(
                string idProveidor)
        {
            Query query =
                db.Collection("facturesProveidor")
                  .WhereEqualTo(
                      "IdProveidor",
                      idProveidor);

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<FacturaProveidor> facturesProveidor =
                new List<FacturaProveidor>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    FacturaProveidor facturaProveidor =
                        document.ConvertTo<FacturaProveidor>();

                    facturesProveidor.Add(
                        facturaProveidor);
                }
            }

            return facturesProveidor;
        }
        public async Task DeleteFacturaProveidor(
            FacturaProveidor facturaProveidor)
        {
            DocumentReference documentReference =
                db.Collection("facturesProveidor")
                  .Document(facturaProveidor.Id);

            await documentReference.DeleteAsync();
        }

        #endregion

        #region LINIA FACTURA PROVEIDOR
        public async Task AddLiniaFacturaProveidor(
            LiniaFacturaProveidor linia)
        {
            if (linia == null)
                throw new ArgumentNullException(nameof(linia));

            if (string.IsNullOrWhiteSpace(linia.IdProducte) ||
                linia.IdProducte == "-")
                throw new ArgumentException(
                    "Cal indicar un producte.");

            if (linia.Quantitat <= 0)
                throw new ArgumentException(
                    "La quantitat ha de ser superior a zero.");

            CollectionReference coleccio =
                db.Collection("liniesFacturaProveidor");

            DocumentReference document = coleccio.Document();

            await document.SetAsync(linia);
        }

        public async Task UpdateLiniaFacturaProveidor(
            LiniaFacturaProveidor liniaFacturaProveidor)
        {
            if (liniaFacturaProveidor == null)
            {
                throw new ArgumentNullException(
                    nameof(liniaFacturaProveidor));
            }

            DocumentReference documentReference =
                db.Collection("liniesFacturaProveidor")
                  .Document(liniaFacturaProveidor.IdProducte);

            await documentReference.SetAsync(
                liniaFacturaProveidor,
                SetOptions.MergeAll);
        }


        public async Task<LiniaFacturaProveidor>
            GetLiniaFacturaProveidorById(
                string idProducte)
        {
            DocumentReference documentReference =
                db.Collection("liniesFacturaProveidor")
                  .Document(idProducte);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<LiniaFacturaProveidor>();
        }


        public async Task<List<LiniaFacturaProveidor>>
            GetAllLiniesFacturaProveidor()
        {
            Query query =
                db.Collection("liniesFacturaProveidor");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<LiniaFacturaProveidor> liniesFacturaProveidor =
                new List<LiniaFacturaProveidor>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    LiniaFacturaProveidor liniaFacturaProveidor =
                        document.ConvertTo<LiniaFacturaProveidor>();

                    liniesFacturaProveidor.Add(
                        liniaFacturaProveidor);
                }
            }

            return liniesFacturaProveidor;
        }


        public async Task DeleteLiniaFacturaProveidor(
            LiniaFacturaProveidor liniaFacturaProveidor)
        {
            DocumentReference documentReference =
                db.Collection("liniesFacturaProveidor")
                  .Document(
                      liniaFacturaProveidor.IdProducte);

            await documentReference.DeleteAsync();
        }

        #endregion

        #region DEVOLUCIONS

        public async Task AddDevolucio(
            Devolucio devolucio)
        {
            if (devolucio == null)
            {
                throw new ArgumentNullException(
                    nameof(devolucio));
            }

            if (string.IsNullOrWhiteSpace(
                devolucio.Id))
            {
                throw new Exception(
                    "La devolució no té un ID.");
            }

            if (string.IsNullOrWhiteSpace(
                devolucio.IdComanda))
            {
                throw new Exception(
                    "La devolució no té una comanda.");
            }

            if (string.IsNullOrWhiteSpace(
                devolucio.IdProducte))
            {
                throw new Exception(
                    "La devolució no té un producte.");
            }

            DocumentReference documentReference =
                db.Collection("devolucions")
                  .Document(devolucio.Id);

            await documentReference.SetAsync(
                devolucio);
        }


        public async Task UpdateDevolucio(
            Devolucio devolucio)
        {
            if (devolucio == null)
            {
                throw new ArgumentNullException(
                    nameof(devolucio));
            }

            DocumentReference documentReference =
                db.Collection("devolucions")
                  .Document(devolucio.Id);

            await documentReference.SetAsync(
                devolucio,
                SetOptions.MergeAll);
        }


        public async Task<Devolucio> GetDevolucioById(
            string id)
        {
            DocumentReference documentReference =
                db.Collection("devolucions")
                  .Document(id);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<Devolucio>();
        }


        public async Task<List<Devolucio>>
            GetAllDevolucions()
        {
            Query query =
                db.Collection("devolucions");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Devolucio> devolucions =
                new List<Devolucio>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Devolucio devolucio =
                        document.ConvertTo<Devolucio>();

                    devolucions.Add(devolucio);
                }
            }

            return devolucions;
        }


        public async Task<List<Devolucio>>
            GetDevolucionsByComanda(
                string idComanda)
        {
            Query query =
                db.Collection("devolucions")
                  .WhereEqualTo(
                      "IdComanda",
                      idComanda);

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Devolucio> devolucions =
                new List<Devolucio>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Devolucio devolucio =
                        document.ConvertTo<Devolucio>();

                    devolucions.Add(devolucio);
                }
            }

            return devolucions;
        }


        public async Task<List<Devolucio>>
            GetDevolucionsByProducte(
                string idProducte)
        {
            Query query =
                db.Collection("devolucions")
                  .WhereEqualTo(
                      "IdProducte",
                      idProducte);

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Devolucio> devolucions =
                new List<Devolucio>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Devolucio devolucio =
                        document.ConvertTo<Devolucio>();

                    devolucions.Add(devolucio);
                }
            }

            return devolucions;
        }


        public async Task DeleteDevolucio(
            Devolucio devolucio)
        {
            if (devolucio == null)
            {
                throw new ArgumentNullException(
                    nameof(devolucio));
            }

            DocumentReference documentReference =
                db.Collection("devolucions")
                  .Document(devolucio.Id);

            await documentReference.DeleteAsync();
        }

        #endregion

        #region MISSATGES
        public async Task AddMissatge(
            Missatge missatge)
        {
            if (missatge == null)
            {
                throw new ArgumentNullException(
                    nameof(missatge));
            }

            if (string.IsNullOrWhiteSpace(
                missatge.Id))
            {
                throw new Exception(
                    "El missatge no té un ID.");
            }

            if (string.IsNullOrWhiteSpace(
                missatge.IdEmissor))
            {
                throw new Exception(
                    "El missatge no té un emissor.");
            }

            if (string.IsNullOrWhiteSpace(
                missatge.IdReceptor))
            {
                throw new Exception(
                    "El missatge no té un receptor.");
            }

            if (string.IsNullOrWhiteSpace(
                missatge.Text))
            {
                throw new Exception(
                    "El missatge no té text.");
            }

            DocumentReference documentReference =
                db.Collection("missatges")
                  .Document(missatge.Id);

            await documentReference.SetAsync(
                missatge);
        }


        public async Task UpdateMissatge(
            Missatge missatge)
        {
            if (missatge == null)
            {
                throw new ArgumentNullException(
                    nameof(missatge));
            }

            DocumentReference documentReference =
                db.Collection("missatges")
                  .Document(missatge.Id);

            await documentReference.SetAsync(
                missatge,
                SetOptions.MergeAll);
        }


        public async Task<Missatge> GetMissatgeById(
            string id)
        {
            DocumentReference documentReference =
                db.Collection("missatges")
                  .Document(id);

            DocumentSnapshot document =
                await documentReference.GetSnapshotAsync();

            if (!document.Exists)
            {
                return null;
            }

            return document.ConvertTo<Missatge>();
        }


        public async Task<List<Missatge>>
            GetAllMissatges()
        {
            Query query =
                db.Collection("missatges");

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Missatge> missatges =
                new List<Missatge>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Missatge missatge =
                        document.ConvertTo<Missatge>();

                    missatges.Add(missatge);
                }
            }

            return missatges
                .OrderBy(m => m.Data)
                .ToList();
        }


        public async Task<List<Missatge>>
            GetMissatgesByEmissor(
                string idEmissor)
        {
            Query query =
                db.Collection("missatges")
                  .WhereEqualTo(
                      "IdEmissor",
                      idEmissor);

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Missatge> missatges =
                new List<Missatge>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Missatge missatge =
                        document.ConvertTo<Missatge>();

                    missatges.Add(missatge);
                }
            }

            return missatges
                .OrderBy(m => m.Data)
                .ToList();
        }


        public async Task<List<Missatge>>
            GetMissatgesByReceptor(
                string idReceptor)
        {
            Query query =
                db.Collection("missatges")
                  .WhereEqualTo(
                      "IdReceptor",
                      idReceptor);

            QuerySnapshot snapshot =
                await query.GetSnapshotAsync();

            List<Missatge> missatges =
                new List<Missatge>();

            foreach (DocumentSnapshot document
                in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Missatge missatge =
                        document.ConvertTo<Missatge>();

                    missatges.Add(missatge);
                }
            }

            return missatges
                .OrderBy(m => m.Data)
                .ToList();
        }


        public async Task<List<Missatge>>
            GetConversa(
                string idUsuari1,
                string idUsuari2)
        {
            List<Missatge> totsElsMissatges =
                await GetAllMissatges();

            List<Missatge> conversa =
                totsElsMissatges
                    .Where(m =>
                        (m.IdEmissor == idUsuari1 &&
                         m.IdReceptor == idUsuari2)
                        ||
                        (m.IdEmissor == idUsuari2 &&
                         m.IdReceptor == idUsuari1))
                    .OrderBy(m => m.Data)
                    .ToList();

            return conversa;
        }

        public async Task DeleteMissatge(
            Missatge missatge)
        {
            if (missatge == null)
            {
                throw new ArgumentNullException(
                    nameof(missatge));
            }

            DocumentReference documentReference =
                db.Collection("missatges")
                  .Document(missatge.Id);

            await documentReference.DeleteAsync();
        }

        #endregion


    }
}