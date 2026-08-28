# Diagrama de Clases UML - LovelyPetShop Clínica Veterinaria

Este documento modela la estructura orientada a objetos de **LovelyPetShop**, reflejando las relaciones de herencia, polimorfismo, abstracción e implementación de interfaces de dominio de acuerdo con los requerimientos de la Historia de Usuario 3.

---

## 1. Diagrama de Clases UML (Mermaid)

```mermaid
classDiagram
    direction TB

    %% Jerarquía de Herencia y Clases Base
    class Animal {
        <<abstract>>
        +string Name
        +string Species
        +int Age
        +double Weight
        +abstract string EmitirSonido()
    }

    class Pet {
        +string Uuid
        +string Breed
        +string Symptoms
        +string OwnerDocumentNumber
        +string OwnerUuid
        +DateTime CreatedAt
        +override string EmitirSonido()
        +string ObtenerResumenRegistro()
    }

    %% Interfaces de Dominio
    class IRegistrable {
        <<interface>>
        +string ObtenerResumenRegistro()
    }

    class INotificable {
        <<interface>>
        +Task~string~ EnviarNotificacionAsync(string mensaje)
    }

    class IAtendible {
        <<interface>>
        +string Atender(Pet pet)
    }

    %% Entidad Propietario
    class Owner {
        +string Uuid
        +string DocumentType
        +string DocumentNumber
        +string Name
        +string Phone
        +string Email
        +string Address
        +DateTime CreatedAt
        +List~Pet~ Pets
        +string ObtenerResumenRegistro()
        +Task~string~ EnviarNotificacionAsync(string mensaje)
    }

    %% Jerarquía de Servicios Veterinarios (Abstracción)
    class ServicioVeterinario {
        <<abstract>>
        +string NombreServicio
        +decimal CostoBase
        +abstract string Atender(Pet pet)
    }

    class ConsultaGeneral {
        +string Motivo
        +override string Atender(Pet pet)
    }

    class Vacunacion {
        +string TipoVacuna
        +override string Atender(Pet pet)
    }

    %% Relaciones
    Animal <|-- Pet : Herencia
    IRegistrable <|.. Pet : Implementa
    IRegistrable <|.. Owner : Implementa
    INotificable <|.. Owner : Implementa
    IAtendible <|.. ServicioVeterinario : Implementa
    ServicioVeterinario <|-- ConsultaGeneral : Especialización
    ServicioVeterinario <|-- Vacunacion : Especialización
    Owner "1" o-- "*" Pet : Posee (1 a N)
```

---

## 2. Explicación de Conceptos POO Aplicados

### A. Herencia y Polimorfismo
* **Clase Base `Animal`**: Encapsula propiedades universales de cualquier animal (`Name`, `Species`, `Age`, `Weight`) y define el método abstracto `EmitirSonido()`.
* **Clase Derivada `Pet`**: Extiende a `Animal`, añadiendo atributos propios de mascotas domésticas de clínica (`Breed`, `OwnerDocumentNumber`, `Symptoms`) y sobrescribe `EmitirSonido()` para emitir sonidos específicos por especie (`"¡Guau guau!"`, `"¡Miau miau!"`, etc.).

### B. Abstracción
* **Clase Abstracta `ServicioVeterinario`**: Provee el contrato base y atributos compartidos (`NombreServicio`, `CostoBase`) forzando la implementación del método `Atender(Pet pet)` en subclases concretas como `ConsultaGeneral` y `Vacunacion`.

### C. Múltiples Interfaces de Dominio
* **`IRegistrable`**: Implementada tanto por `Owner` como por `Pet`, estandarizando la obtención de resúmenes de registro.
* **`INotificable`**: Implementada por `Owner` para el envío no bloqueante de avisos por mensajería o correo electrónico.
* **`IAtendible`**: Implementada por `ServicioVeterinario` para estandarizar la atención a pacientes.

### D. Encapsulación y Asociación 1 a N
* Un `Owner` mantiene una lista fuertemente tipada de mascotas asociadas (`List<Pet> Pets`), manteniendo la integridad de las relaciones y protegiendo el acceso a los datos.
