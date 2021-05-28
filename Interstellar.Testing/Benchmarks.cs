using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Interstellar.Schema;
using Interstellar.SqlServer;
using SqlKata.Compilers;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Interstellar.Testing
{
    [SimpleJob(RuntimeMoniker.NetCoreApp50, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Net48)]
    public class Benchmarks
    {
        private QueryFactory _factory;
        private SqlKata.Execution.QueryFactory _kataFactory;

        [GlobalSetup]
        public void Setup()
        {
            var builder = new DbSchemaBuilder();
            builder
                .SchemaFor<SaldoDettaglio>(b => b
                    .Source("dbo.SALDI_DETTAGLIO")
                    .Column(x => x.Stabilimento, "STAB")
                    .Column(x => x.Magazzino, "MAGA")
                    .Column(x => x.Progressivo, "PROG")
                    .Column(x => x.Articolo, "ARTI")
                    .Column(x => x.IdLotto, "IdLotto"))
                .SchemaFor<Articolo>(b => b
                    .Source("dbo.ARTICOLI")
                    .Column(x => x.CodiceArticolo, "ARTI")
                    .Column(x => x.Descrizione, "DSCR")
                    .Column(x => x.TipoArticolo, "TIPO_ARTI"));

            builder
                .ForeignKey<SaldoTestata, SaldoDettaglio>(b => b
                    .Column(sd => sd.Stabilimento, st => st.Stabilimento)
                    .Column(sd => sd.Magazzino, st => st.Magazzino)
                    .Column(sd => sd.Progressivo, st => st.Progressivo))
                .ForeignKey<Articolo, SaldoDettaglio>(b => b
                    .Column(a => a.CodiceArticolo, sd => sd.Articolo));

            var compiler = new QueryCompiler(builder);
            var executor = new QueryExecutor("Data Source =.\\; Initial Catalog = EasyStock_Dev; User ID = sa; Password = m4dl4b2013;");
            _factory = new QueryFactory(compiler, executor);

            _kataFactory = new SqlKata.Execution.QueryFactory(
                new SqlConnection("Data Source =.\\; Initial Catalog = EasyStock_Dev; User ID = sa; Password = m4dl4b2013;"),
                new SqlServerCompiler());
        }

        [Benchmark]
        public async Task QueryManyAsync()
        {
            IEnumerable<SaldoDettaglio> result =
                await _factory.GetManyAsync<SaldoDettaglio>(q => q
                .Select<SaldoDettaglio, string>(sd => sd.Stabilimento)
                .Select<SaldoDettaglio, string>(sd => sd.Magazzino)
                .Select<SaldoDettaglio, string>(sd => sd.Progressivo)
                .Select<SaldoDettaglio, string>(sd => sd.Articolo)
                .Select<SaldoDettaglio, int>(sd => sd.IdLotto)
                .From<SaldoDettaglio>(sd => sd))
                .ConfigureAwait(false);
        }

        [Benchmark]
        public async Task QueryManySqlKataAsync()
        {
            IEnumerable<SaldoDettaglio> result =
               await _kataFactory.GetAsync<SaldoDettaglio>(
                   new SqlKata.Query()
                   .Select(
                       $"STAB AS {nameof(SaldoDettaglio.Stabilimento)}",
                       $"MAGA AS {nameof(SaldoDettaglio.Magazzino)}",
                       $"PROG AS {nameof(SaldoDettaglio.Progressivo)}",
                       $"ARTI AS {nameof(SaldoDettaglio.Articolo)}",
                       $"IdLotto AS {nameof(SaldoDettaglio.IdLotto)}")
                   .From("dbo.SALDI_DETTAGLIO"))
               .ConfigureAwait(false);
        }
    }
}
