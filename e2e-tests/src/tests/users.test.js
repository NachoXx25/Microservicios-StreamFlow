const ApiClient = require('../utils/api-client');


describe('User Microservice E2E Tests', () => {
  let apiClient;
  let testUserId;
  let authToken;

    beforeAll(async () => {
        apiClient = new ApiClient();
        user = {
            firstName: 'Admin',
            lastName: 'Test',
            email: 'juana@gmail.com',
            password: 'Password123!',
            confirmPassword: 'Password123!',
            role: "Cliente"
        };
        try {
        try {
          await apiClient.createUser(user);
        } catch (error) {
          if (error.status !== 409 && error.status !== 400) {
            throw error;
          }
        }

        const loginResponse = await apiClient.login(user.email, user.password);
        expect(loginResponse.status).toBe(200);
        expect(apiClient.authToken).toBeDefined();
        
        console.log('Usuario creado y autenticado correctamente');
      } catch (error) {
        throw error;
      }
    });

    afterAll(async () => {
        if (testUserId) {
        try {
            await apiClient.deleteUser(testUserId);
        } catch (error) {
            console.log('Error de limpieza:', error.message);
        }
        }
    });

    describe('POST /auth/login', () => {
        test('Prueba con credenciales correctas', async () => {
        const loginData = {
            email: 'juana@gmail.com',
            password: 'Password123!'
        };

        const response = await apiClient.login(loginData.email, loginData.password);
        
        expect(response.status).toBe(200);
        expect(response.data).toBeDefined();
    });

    test('Prueba de error con credenciales incorrectas', async () => {
        const loginData = {
            email: 'juana@gmail.com',
            password: 'HolaChiquillosdelYT'
        };

        try {
          await apiClient.login(loginData.email, loginData.password);
          expect(true).toBe(false);
        } 
        catch (error) {
          expect(error.status).toBe(400);
          expect(error.data).toHaveProperty('error');
        }
        });
    });

    describe('POST /usuarios', () => {
        test('Prueba de exito', async () => {
            const newUser = apiClient.generateTestUser();
            console.log('User:', {
              firstName: newUser.firstName,
              lastName: newUser.lastName,
              email: newUser.email,
              passwordLength: newUser.password.length,
              role: newUser.roleId === 1 ? 'Admin' : 'Cliente'
            });
            
            const response = await apiClient.createUser(newUser);
            
            expect(response.status).toBe(201);
            
            testUserId = response.data.id;
            console.log(`Prueba de usuario creado con id: ${testUserId}`);
        });

        test('Prueba de error con nombre de usuario corto', async () => {
          const invalidUser = apiClient.generateTestUser();
          invalidUser.firstName = 'A';
        
          try {
            await apiClient.createUser(invalidUser);
            expect(true).toBe(false); 
          } 
          catch (error) {
            expect(error.status).toBe(400);
            expect(error.data).toHaveProperty('error');
            console.log('Validation error:', error.data.error);
          }
        });
    });

    describe('GET /usuarios/{id}', () => {
    test('Prueba de exito para obtener usuario por id', async () => {
     if (!testUserId) {
        const newUser = apiClient.generateTestUser();
        const createResponse = await apiClient.createUser(newUser);
        testUserId = createResponse.data.id;
      }
      
      expect(testUserId).toBeDefined();
      
      const response = await apiClient.getUserById(testUserId);
      
      expect(response.status).toBe(200);
 
    });

    test('Prueba de error con id que no debería ser existente', async () => {
      const nonExistentId = 999999;
      
      try {
        await apiClient.getUserById(nonExistentId);
        expect(true).toBe(false);
      } catch (error) {
        expect(error.status).toBe(404);
        expect(error.data).toHaveProperty('error');
      }

    });
  });

  describe('PATCH /usuarios/{id}', () => {
    test('Prueba de exito para actualizar usuario', async () => {
      if (!testUserId) {
          const newUser = apiClient.generateTestUser();
          const createResponse = await apiClient.createUser(newUser);
          testUserId = createResponse.data.id;
      }

      const updateData = { firstName: 'SoloNombre' };
      const response = await apiClient.updateUser(testUserId, updateData);
      
      expect(response.status).toBe(200);
    });

    test('Prueba de error con id no válido', async () => {
      const invalidId = 'not-a-number';
      const updateData = {
        firstName: 'Test'
      };

      try {
        await apiClient.updateUser(invalidId, updateData);
        expect(true).toBe(false); 
      } catch (error) {
        expect(error.status).toBe(400);
        expect(error.data).toHaveProperty('error');
        expect(error.data.error).toContain('ID debe ser un número entero positivo');
      }
    });
  });

  describe('GET /usuarios', () => {
    test('Prueba de exito para obtener todos los usuarios', async () => {
      const response = await apiClient.getAllUsers();
      
      expect(response.status).toBe(200);
      expect(Array.isArray(response.data)).toBe(true);
      expect(response.data.length).toBeGreaterThan(0);
      
      
      if (testUserId) {
        const foundUser = response.data.find(user => user.id === testUserId);
        console.log('Found user:', foundUser ? 'Yes' : 'No');
        expect(foundUser).toBeDefined();
      }
    });

    test('Prueba de error para obtener usuarios (sin credenciales)', async () => {
      
      const originalToken = apiClient.authToken;
      apiClient.setAuthToken(null);
      
      try {
        await apiClient.getAllUsers();
        expect(true).toBe(false); 
      } catch (error) {
        expect(error.status).toBe(401);
        expect(error.data).toHaveProperty('error');

        apiClient.setAuthToken(originalToken);
      }
    });
  });
  
  describe('DELETE /usuarios/{id}', () => {
    test('Prueba de éxito para eliminar usuario', async () => {
      
      if (!testUserId) {
        const userToDelete = apiClient.generateTestUser();
        const createResponse = await apiClient.createUser(userToDelete);
        testUserId = createResponse.data.id;
      }

      const response = await apiClient.deleteUser(testUserId);
      expect(response.status).toBe(204);
      testUserId = null; 
    });
      test('Prueba de error para eliminar usuario con id no existente', async () => {
      const nonExistentId = 999999;
      
      try {
        await apiClient.deleteUser(nonExistentId);
        expect(true).toBe(false);
      } catch (error) {
        expect([401, 404]).toContain(error.status);
        expect(error.data).toHaveProperty('error');
      }
    });
  });
  
});