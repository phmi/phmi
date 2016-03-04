using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiModel.Interfaces;
using PHmiResources.Loc;

namespace PHmiModel.Entities
{
    public class NamedEntity : Entity, INamedEntity
    {
        private string _name;

        [Column("name")]
        [Display(Name = "Name", ResourceType = typeof (Res))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof (Res))]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(this, e => e.Name);
            }
        }
    }
}
