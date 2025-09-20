import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Directorio a procesar
const handoverDir = path.join(__dirname, 'src/components/handover');

// Función para procesar archivos recursivamente
function processDirectory(dir) {
  const files = fs.readdirSync(dir);

  for (const file of files) {
    const filePath = path.join(dir, file);
    const stat = fs.statSync(filePath);

    if (stat.isDirectory() && file !== 'node_modules') {
      processDirectory(filePath);
    } else if (file.endsWith('.tsx') || file.endsWith('.ts')) {
      processFile(filePath);
    }
  }
}

// Función para procesar un archivo individual
function processFile(filePath) {
  let content = fs.readFileSync(filePath, 'utf8');
  let modified = false;

  // 1. Corregir imports duplicados de React
  if (content.includes('import type React from "react";') && content.includes('import {')) {
    content = content.replace(/import type React from "react";\n/g, '');
    modified = true;
  }

  // 2. Agregar tipos de retorno a funciones arrow que no los tienen
  const arrowFunctionRegex = /(const\s+\w+\s*=\s*\([^)]*\)\s*=>)/g;
  content = content.replace(arrowFunctionRegex, (match, p1) => {
    // Solo agregar tipo si no tiene uno y no es una función que retorna JSX
    if (!match.includes(':') && !content.includes('JSX.Element')) {
      return match + ': void';
    }
    return match;
  });

  // 3. Agregar tipos de retorno a funciones export que no los tienen
  const exportFunctionRegex = /(export\s+(const|function)\s+\w+\s*\([^)]*\)\s*\{)/g;
  content = content.replace(exportFunctionRegex, (match) => {
    if (!match.includes(':') && !content.includes('JSX.Element')) {
      return match.replace('{', ': JSX.Element {');
    }
    return match;
  });

  // 4. Renombrar variables 'e' a 'event'
  content = content.replace(/\((e):\s*React\./g, '(event: React.');
  content = content.replace(/\be\./g, 'event.');

  // 5. Eliminar prefijos _ de parámetros no utilizados
  content = content.replace(/\(_(\w+):\s*[^)]*\)/g, '($1: $2)');

  if (modified) {
    fs.writeFileSync(filePath, content, 'utf8');
    console.log(`Fixed: ${filePath}`);
  }
}

// Ejecutar el procesamiento
console.log('Starting lint error fixes...');
processDirectory(handoverDir);
console.log('Finished processing handover directory');
