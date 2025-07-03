import type { FC } from 'react';

type SumCalculatorProps = {
  a: number;
  b: number;
};

export const SumCalculator: FC<SumCalculatorProps> = ({ a, b }) => {
  const sum = a + b;

  return (
    <div className="flex w-full max-w-md flex-col items-center justify-center rounded-lg border border-gray-200 bg-white p-6 shadow-md transition-colors dark:border-gray-700 dark:bg-gray-800 sm:p-8">
      <h2 className="mb-4 text-xl font-bold text-gray-800 dark:text-white sm:text-2xl">
        Sum Calculator
      </h2>
      <div className="flex w-full items-center justify-center space-x-2 text-lg sm:space-x-4 sm:text-xl">
        <div className="flex h-16 w-16 items-center justify-center rounded-md bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 sm:h-20 sm:w-20">
          <span className="text-2xl font-semibold sm:text-3xl">{a}</span>
        </div>
        <span className="text-2xl font-light text-gray-500 dark:text-gray-400 sm:text-3xl">
          +
        </span>
        <div className="flex h-16 w-16 items-center justify-center rounded-md bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200 sm:h-20 sm:w-20">
          <span className="text-2xl font-semibold sm:text-3xl">{b}</span>
        </div>
        <span className="text-2xl font-light text-gray-500 dark:text-gray-400 sm:text-3xl">
          =
        </span>
        <div className="flex h-16 w-16 items-center justify-center rounded-md bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200 sm:h-20 sm:w-20">
          <span className="text-2xl font-semibold sm:text-3xl">{sum}</span>
        </div>
      </div>
      <p className="mt-4 text-sm text-gray-600 dark:text-gray-300">
        This component adds two numbers.
      </p>
    </div>
  );
}; 