import hashlib
import datetime
import random
import time

class Node:
    def __init__(self, partida, cuerpo, firma_digital):
        self.partida = partida
        self.cuerpo = cuerpo  # Una lista de enteros
        self.firma_digital = firma_digital
        self.next = None # Referencia al siguiente nodo

    def __str__(self):
        # Para facilitar la impresión y depuración
        return (f"Partida: {self.partida}\n"
                f"Cuerpo: {self.cuerpo}\n"
                f"Firma Digital: {self.firma_digital}\n")

def generate_sha256_hash(data_string):
    """Genera un hash SHA256 de una cadena de texto."""
    return hashlib.sha256(data_string.encode('utf-8')).hexdigest()

def build_linked_list(n, k):
    """
    Construye una lista enlazada según las especificaciones.
    n: número de nodos
    k: número de elementos aleatorios en el cuerpo de cada nodo
    """
    head = None
    current_node = None
    previous_node_signature = None # Para la "partida" de los nodos subsiguientes

    for i in range(n):
        # 1. Generar cuerpo: k elementos aleatorios
        body_elements = [random.randint(1, 100000) for _ in range(k)]
        body_string = " ".join(map(str, body_elements)) # Convertir a string para la firma

        # 2. Determinar 'partida'
        if i == 0:
            # Primer nodo: firma SHA256 de la fecha y hora actual
            current_datetime_str = datetime.datetime.now().strftime("%d/%m/%Y %H:%M:%S")
            partida_value = generate_sha256_hash(current_datetime_str)
        else:
            # Nodos subsiguientes: copia de la firma digital del nodo precedente
            partida_value = previous_node_signature

        # 3. Calcular 'firma_digital' del nodo actual
        # Concatenar partida con los elementos del cuerpo separados por espacio
        # (asegurarse de que partida_value sea una cadena de texto)
        content_to_hash = f"{partida_value} {body_string}"
        current_node_signature = generate_sha256_hash(content_to_hash)

        # 4. Crear el nodo
        new_node = Node(partida_value, body_elements, current_node_signature)

        # 5. Enlazar el nodo a la lista
        if head is None:
            head = new_node
        else:
            current_node.next = new_node

        current_node = new_node
        previous_node_signature = current_node_signature # Actualizar para el próximo nodo

    return head

def run_scenario(n, k):
    """Ejecuta un escenario y mide el tiempo."""
    start_time = time.perf_counter() # Usa perf_counter para mayor precisión
    linked_list_head = build_linked_list(n, k)
    end_time = time.perf_counter()
    execution_time_ms = (end_time - start_time) * 1000 # Convertir a milisegundos

    return execution_time_ms


if __name__ == "__main__":
    print("--- Ejecutando escenarios en Python ---")

    scenarios = [
        {"n": 3, "k": 4},
        {"n": 10, "k": 200},
        {"n": 200, "k": 10}
    ]

    python_results = {}
    for scenario in scenarios:
        n = scenario["n"]
        k = scenario["k"]
        print(f"\nEscenario: n={n}, k={k}")

        # Ejecutar varias veces y tomar el promedio para mayor precisión
        # Especialmente útil para escenarios muy rápidos
        num_runs = 5
        total_time = 0
        for _ in range(num_runs):
            total_time += run_scenario(n, k)
        
        avg_time = total_time / num_runs
        python_results[f"n={n}, k={k}"] = avg_time
        print(f"Tiempo promedio: {avg_time:.4f} ms")

    print("\n--- Resultados de Python ---")
    for scenario_str, time_ms in python_results.items():
        print(f"{scenario_str}: {time_ms:.4f} ms")
