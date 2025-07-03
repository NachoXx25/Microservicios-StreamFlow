beforeEach(() => {
  console.log(`Corriendo pruebas 🥵: ${expect.getState().currentTestName}`);
});

afterEach(() => {
  console.log(`Prueba completada 😱: ${expect.getState().currentTestName}`);
});

global.generateTestData = {
  user: () => ({
    firstName: 'Test',
    lastName: 'User',
    email: `test.${Date.now()}@example.com`,
    password: 'TestPass123!',
    roleId: 2
  }),
  
  admin: () => ({
    firstName: 'Admin',
    lastName: 'Test',
    email: `admin.${Date.now()}@example.com`,
    password: 'Password123!',
    roleId: 1
  })
};

global.sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));