using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Genome
{
    public GenomeTraits traits { get; private set; }

    public Genome()
    {
        this.traits = new GenomeTraits();
    }

    public Genome(GenomeTraits traits)
    {
        this.traits = traits;
    }

    internal void Mutate()
    {
        traits.Mutate();
    }
}

