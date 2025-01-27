<template>
  <div class="app-container">
    <div class="header">
      <h2>应用程序管理</h2>
      <el-button type="primary" @click="showCreateDialog">新增应用</el-button>
    </div>

    <el-table :data="applications" v-loading="loading">
      <el-table-column prop="name" label="应用名称" />
      <el-table-column prop="version" label="版本" width="100" />
      <el-table-column prop="description" label="描述" />
      <el-table-column label="操作" width="200">
        <template #default="{ row }">
          <el-button type="primary" size="small" @click="handleEdit(row)">编辑</el-button>
          <el-button type="danger" size="small" @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      :current-page="currentPage"
      :page-size="pageSize"
      :total="total"
      @current-change="handlePageChange"
      @size-change="handleSizeChange"
      layout="total, prev, pager, next"
      class="pagination"
    />

    <!-- 新增/编辑对话框 -->
    <el-dialog
      :title="dialogTitle"
      v-model="dialogVisible"
      width="500px"
    >
      <el-form :model="form" :rules="rules" ref="formRef" label-width="100px">
        <el-form-item label="应用名称" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="版本" prop="version">
          <el-input v-model="form.version" />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input type="textarea" v-model="form.description" />
        </el-form-item>
        <el-form-item label="官方网站">
          <el-input v-model="form.officialUrl" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="submitting">
          确定
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import axios from 'axios'

export default {
  name: 'ApplicationList',
  setup() {
    const applications = ref([])
    const loading = ref(false)
    const dialogVisible = ref(false)
    const dialogTitle = ref('')
    const submitting = ref(false)
    const currentPage = ref(1)
    const pageSize = ref(10)
    const total = ref(0)

    const form = reactive({
      id: '',
      name: '',
      version: '',
      description: '',
      officialUrl: ''
    })

    const rules = {
      name: [{ required: true, message: '请输入应用名称', trigger: 'blur' }],
      version: [{ required: true, message: '请输入版本号', trigger: 'blur' }],
      description: [{ required: true, message: '请输入应用描述', trigger: 'blur' }]
    }

    const loadApplications = async () => {
      try {
        loading.value = true
        const response = await axios.get(`http://localhost:5000/api/application/paged`, {
          params: {
            pageIndex: currentPage.value,
            pageSize: pageSize.value
          }
        })
        applications.value = response.data.items
        total.value = response.data.totalCount
      } catch (error) {
        ElMessage.error('加载应用列表失败')
      } finally {
        loading.value = false
      }
    }

    const handlePageChange = (page) => {
      currentPage.value = page
      loadApplications()
    }

    const handleSizeChange = (size) => {
      pageSize.value = size
      loadApplications()
    }

    const showCreateDialog = () => {
      Object.keys(form).forEach(key => form[key] = '')
      dialogTitle.value = '新增应用'
      dialogVisible.value = true
    }

    const handleEdit = (row) => {
      Object.assign(form, row)
      dialogTitle.value = '编辑应用'
      dialogVisible.value = true
    }

    const handleDelete = async (row) => {
      try {
        await ElMessageBox.confirm('确定要删除该应用吗？', '提示', {
          type: 'warning'
        })
        await axios.delete(`http://localhost:5000/api/application/${row.id}`)
        ElMessage.success('删除成功')
        loadApplications()
      } catch (error) {
        if (error !== 'cancel') {
          ElMessage.error('删除失败')
        }
      }
    }

    const handleSubmit = async () => {
      try {
        submitting.value = true
        if (form.id) {
          await axios.put(`http://localhost:5000/api/application/${form.id}`, form)
        } else {
          await axios.post('http://localhost:5000/api/application', form)
        }
        ElMessage.success(form.id ? '更新成功' : '创建成功')
        dialogVisible.value = false
        loadApplications()
      } catch (error) {
        ElMessage.error(form.id ? '更新失败' : '创建失败')
      } finally {
        submitting.value = false
      }
    }

    onMounted(() => {
      loadApplications()
    })

    return {
      applications,
      loading,
      dialogVisible,
      dialogTitle,
      form,
      rules,
      submitting,
      currentPage,
      pageSize,
      total,
      showCreateDialog,
      handleEdit,
      handleDelete,
      handleSubmit,
      handlePageChange,
      handleSizeChange
    }
  }
}
</script>

<style scoped>
.app-container {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.pagination {
  margin-top: 20px;
  text-align: right;
}
</style> 
