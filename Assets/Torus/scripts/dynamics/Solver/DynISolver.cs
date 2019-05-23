using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface DynISolver
{
    void Solve(float dt, List<DynElement> elements);
}
