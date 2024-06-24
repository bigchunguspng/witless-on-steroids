using System.Linq;
using Witlesss.Generation;

namespace Witlesss.Services
{
    public class FusionCollab
    {
        private readonly GenerationPack _packSource, _packTarget;

        public FusionCollab(Witless witless, GenerationPack pack)
        {
            witless.Backup();

            _packSource = pack;
            _packTarget = witless.Pack;
        }

        public void Fuse()
        {
            // update vocabulary
            var ids = _packSource.Vocabulary.Select(word => _packTarget.GetOrAddWord_ReturnID(word)).ToList();

            // update transitions
            foreach (var id in ids)
            {
                var tableTarget = _packTarget.GetTableByID(id);
                var tableSource = _packSource.GetTableByID(id);
                foreach (var transition in tableSource)
                {
                    tableTarget.Put(ids[transition.WordID], transition.Chance);
                }
            }
        }
    }
}