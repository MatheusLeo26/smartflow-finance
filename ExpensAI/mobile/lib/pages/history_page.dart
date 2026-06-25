import 'package:flutter/material.dart';

class HistoryPage extends StatelessWidget {
  const HistoryPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final List<Map<String, dynamic>> items = List.generate(
      50,
      (index) => {
        'id': index,
        'title': index % 2 == 0 ? 'Uber Trip' : 'Mercado Central',
        'category': index % 2 == 0 ? 'Transporte' : 'Alimentação',
        'value': (index + 1) * 12.50,
        'isSynced': index % 5 != 0,
      },
    );

    return Scaffold(
      appBar: AppBar(
        title: const Text('Histórico de Gastos'),
      ),
      body: ListView.builder(
        itemCount: items.length,
        itemBuilder: (context, index) {
          final item = items[index];
          return Card(
            margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
            child: ListTile(
              leading: CircleAvatar(
                backgroundColor: item['category'] == 'Transporte' 
                    ? Colors.blue.shade100 
                    : Colors.green.shade100,
                child: Icon(
                  item['category'] == 'Transporte' ? Icons.directions_car : Icons.shopping_basket,
                  color: item['category'] == 'Transporte' ? Colors.blue : Colors.green,
                ),
              ),
              title: Text(item['title']),
              subtitle: Text(item['category']),
              trailing: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    '- R\$ ${item['value'].toStringAsFixed(2)}',
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                  ),
                  const SizedBox(width: 8),
                  if (!item['isSynced'])
                    const Tooltip(
                      message: 'Aguardando conexão para sincronizar',
                      child: Icon(Icons.cloud_off_outlined, color: Colors.orange, size: 18),
                    )
                  else
                    const Icon(Icons.cloud_done_outlined, color: Colors.green, size: 18),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
