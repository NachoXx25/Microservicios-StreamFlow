const ApiClient = require('../utils/api-client');


describe('User Microservice E2E Tests', () => {
  let apiClient;
  let testUserId;
  let authToken;

    beforeAll(async () => {
        apiClient = new ApiClient();


        const adminUser = {
        firstName: 'Admin',
        lastName: 'Test',
        email: 'admin@test.com',
        password: 'Password123!',
        roleId: 1
        };

        try {
        await apiClient.createUser(adminUser);
        const loginResponse = await apiClient.login(adminUser.email, adminUser.password);
        authToken = loginResponse.data.token;
        expect(authToken).toBeDefined();
        } 
        catch (error) {
        console.error('Error:', error);
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
            email: 'admin@test.com',
            password: 'Password123!'
        };

        const response = await apiClient.login(loginData.email, loginData.password);
        
        expect(response.status).toBe(200);
        expect(response.data).toHaveProperty('token');
        expect(response.data).toHaveProperty('user');
        expect(response.data.user.email).toBe(loginData.email);
    });

    test('Prueba de error con credenciales incorrectas', async () => {
        const loginData = {
            email: 'admin@test.com',
            password: 'HolaChiquillosdelYT'
        };

        const error = await apiClient.login(loginData.email, loginData.password);
        
        expect(error.status).toBe(401);
        expect(error.data).toHaveProperty('message');
        });
    });

    describe('POST /usuarios', () => {
        test('Prueba de exito', async () => {
        const newUser = {
            firstName: 'John',
            lastName: 'Doe',
            email: `john.doe.${Date.now()}@test.com`,
            password: 'SecurePass123!',
            roleId: 2
        };

        const response = await apiClient.createUser(newUser);
        
        expect(response.status).toBe(201);
        expect(response.data).toHaveProperty('id');
        expect(response.data.email).toBe(newUser.email);
        expect(response.data.firstName).toBe(newUser.firstName);
        
        testUserId = response.data.id;
        });

        test('Prueba de error con usuario ya existente', async () => {
        const duplicateUser = {
            firstName: 'Jane',
            lastName: 'Doe',
            email: 'admin@test.com', 
            password: 'SecurePass123!',
            roleId: 2
        };

        const error = await apiClient.createUser(duplicateUser);
        
        expect(error.status).toBe(400);
        expect(error.data).toHaveProperty('message');
        });
    });

    describe('GET /usuarios/{id}', () => {
    test('Prueba de exito para obtener usuario por id', async () => {
      expect(testUserId).toBeDefined();
      
      const response = await apiClient.getUserById(testUserId);
      
      expect(response.status).toBe(200);
      expect(response.data).toHaveProperty('id', testUserId);
      expect(response.data).toHaveProperty('firstName');
      expect(response.data).toHaveProperty('email');
    });

    test('Prueba de error con id que no debería ser existente', async () => {
      const nonExistentId = 999999;
      
      const error = await apiClient.getUserById(nonExistentId);
      
      expect(error.status).toBe(404);
      expect(error.data).toHaveProperty('message');
    });
  });

  describe('PATCH /usuarios/{id}', () => {
    test('Prueba de exito para actualizar usuario', async () => {
      expect(testUserId).toBeDefined();
      
      const updateData = {
        firstName: 'John Updated',
        lastName: 'Doe Updated'
      };

      const response = await apiClient.updateUser(testUserId, updateData);
      
      expect(response.status).toBe(200);
      expect(response.data.firstName).toBe(updateData.firstName);
      expect(response.data.lastName).toBe(updateData.lastName);
    });

    test('Prueba de error con id no válido', async () => {
      const invalidId = 'not-a-number';
      const updateData = {
        firstName: 'Test'
      };

      const error = await apiClient.updateUser(invalidId, updateData);
      
      expect(error.status).toBe(400);
      expect(error.data).toHaveProperty('message');
    });
  });

  describe('GET /usuarios', () => {
    test('Prueba de exito para obtener todos los usuarios', async () => {
      const response = await apiClient.getAllUsers();
      
      expect(response.status).toBe(200);
      expect(Array.isArray(response.data)).toBe(true);
      expect(response.data.length).toBeGreaterThan(0);
      
      
      const foundUser = response.data.find(user => user.id === testUserId);
      expect(foundUser).toBeDefined();
    });

    test('Prueba de error para obtener usuarios (sin credenciales)', async () => {
      
      const originalToken = apiClient.authToken;
      apiClient.setAuthToken(null);
      
      const error = await apiClient.getAllUsers();
      
      expect(error.status).toBe(401);
      expect(error.data).toHaveProperty('message');
      
      apiClient.setAuthToken(originalToken);
    });
  });
  describe('DELETE /usuarios/{id}', () => {
    test('Prueba de error para eliminar usuario con id no existente', async () => {
      const nonExistentId = 999999;
      
      const error = await apiClient.deleteUser(nonExistentId);
      
      expect(error.status).toBe(404);
      expect(error.data).toHaveProperty('message');
    });

    test('Prueba de exito para eliminar usuario', async () => {
      expect(testUserId).toBeDefined();
      
      const response = await apiClient.deleteUser(testUserId);
      
      expect(response.status).toBe(200);
      expect(response.data).toHaveProperty('message');
      
      // Verificar que el usuario fue eliminado
      const error = await apiClient.getUserById(testUserId);
      expect(error.status).toBe(404);
      
      testUserId = null; 
    });
  });
});