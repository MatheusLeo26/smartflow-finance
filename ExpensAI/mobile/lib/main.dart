import 'package:flutter/provider.dart'; // fallback if using basic routes
import 'package:flutter/material.dart';
import 'pages/dashboard_page.dart';
import 'pages/upload_page.dart';
import 'pages/history_page.dart';

void main() {
  runApp(const ExpensAIApp());
}

class ExpensAIApp extends StatelessWidget {
  const ExpensAIApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'ExpensAI',
      theme: ThemeData(
        primarySwatch: Colors.blue,
        useMaterial3: true,
        fontFamily: 'Roboto',
      ),
      initialRoute: '/',
      routes: {
        '/': (context) => const DashboardPage(),
        '/upload': (context) => const UploadPage(),
        '/history': (context) => const HistoryPage(),
      },
    );
  }
}
