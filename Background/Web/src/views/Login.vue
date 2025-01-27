<template>
  <div class="login-container">
    <el-card class="login-card">
      <h2>系统登录</h2>
      <el-form :model="loginForm" :rules="rules" ref="loginFormRef">
        <el-form-item prop="username">
          <el-input 
            v-model="loginForm.username" 
            placeholder="用户名"
            :prefix-icon="User">
          </el-input>
        </el-form-item>
        <el-form-item prop="password">
          <el-input 
            v-model="loginForm.password" 
            type="password" 
            placeholder="密码"
            :prefix-icon="Lock">
          </el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSubmit" :loading="loading" style="width: 100%">
            登录
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'
import axios from 'axios'

export default {
  name: 'LoginPage',
  setup() {
    const router = useRouter()
    const loginFormRef = ref(null)
    const loginForm = reactive({
      username: '',
      password: ''
    })
    const loading = ref(false)
    
    const rules = {
      username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
      password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
    }

    const handleSubmit = async () => {
      if (!loginFormRef.value) return
      
      await loginFormRef.value.validate(async (valid) => {
        if (!valid) return
        try {
          loading.value = true
          const response = await axios.post('http://localhost:5000/api/auth/login', loginForm)
          if (response.status === 200) {
            localStorage.setItem('isAuthenticated', 'true')
            ElMessage.success('登录成功')
            router.push('/applications')
          }
        } catch (error) {
          ElMessage.error('用户名或密码错误')
        } finally {
          loading.value = false
        }
      })
    }

    return {
      loginForm,
      loginFormRef,
      rules,
      loading,
      handleSubmit,
      User,
      Lock
    }
  }
}
</script>

<style scoped>
.login-container {
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #f5f7fa;
}

.login-card {
  width: 400px;
}

h2 {
  text-align: center;
  margin-bottom: 30px;
}
</style> 
